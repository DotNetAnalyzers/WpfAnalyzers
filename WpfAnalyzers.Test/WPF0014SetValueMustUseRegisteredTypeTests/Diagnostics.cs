namespace WpfAnalyzers.Test.WPF0014SetValueMustUseRegisteredTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class Diagnostics
    {
        [TestCase("SetValue(BarProperty, ↓1.0)")]
        [TestCase("SetCurrentValue(BarProperty, ↓1.0)")]
        [TestCase("this.SetValue(BarProperty, ↓1.0)")]
        [TestCase("this.SetCurrentValue(BarProperty, ↓1.0)")]
        [TestCase("SetValue(BarProperty, ↓null)")]
        [TestCase("SetCurrentValue(BarProperty, ↓null)")]
        [TestCase("SetValue(BarProperty, ↓\"abc\")")]
        [TestCase("SetCurrentValue(BarProperty, ↓\"abc\")")]
        public void DependencyProperty(string setCall)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }

        public void Meh()
        {
            this.SetValue(BarProperty, ↓1);
        }
    }
}";
            testCode = testCode.AssertReplace("this.SetValue(BarProperty, ↓1)", setCall);
            AnalyzerAssert.Diagnostics<WPF0014SetValueMustUseRegisteredType>(testCode);
        }

        [TestCase("this.SetValue(BarProperty, ↓1.0);")]
        [TestCase("this.SetCurrentValue(BarProperty, ↓1.0);")]
        public void DependencyPropertyGeneric(string setValueCall)
        {
            var fooControlGeneric = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl<T> : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(T),
            typeof(FooControl<T>),
            new PropertyMetadata(default(T)));

        public T Bar
        {
            get { return (T)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";

            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : FooControl<int>
    {
        public void Meh()
        {
            this.SetValue(BarProperty, ↓1.0);
        }
    }
}";
            testCode = testCode.AssertReplace("this.SetValue(BarProperty, ↓1.0)", setValueCall);
            AnalyzerAssert.Diagnostics<WPF0014SetValueMustUseRegisteredType>(fooControlGeneric, testCode);
        }

        [TestCase("this.SetValue(BarProperty, ↓1);")]
        [TestCase("this.SetCurrentValue(BarProperty, ↓1);")]
        public void DependencyPropertyAddOwner(string setValueCall)
        {
            var fooCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(bool),
            typeof(Foo),
            new PropertyMetadata(default(bool)));

        public static void SetBar(FrameworkElement element, bool value)
        {
            element.SetValue(BarProperty, value);
        }

        public static bool GetBar(FrameworkElement element)
        {
            return (bool)element.GetValue(BarProperty);
        }
    }
}";

            var fooControlPart1 = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(
            typeof(FooControl),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnVolumeChanged,
                OnVolumeCoerce));

        public bool Bar
        {
            get { return (bool)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        private static object OnVolumeCoerce(DependencyObject d, object basevalue)
        {
            return basevalue;
        }

        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";

            var fooControlPart2 = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class FooControl
    {
        public FooControl()
        {
            this.SetValue(BarProperty, 1);
        }
    }
}";
            fooControlPart2 = fooControlPart2.AssertReplace("this.SetValue(BarProperty, 1);", setValueCall);
            AnalyzerAssert.Diagnostics<WPF0014SetValueMustUseRegisteredType>(fooCode, fooControlPart1, fooControlPart2);
        }

        [TestCase("SetValue")]
        [TestCase("SetCurrentValue")]
        public void DependencyPropertyOfInterfaceType(string methodName)
        {
            var iFooCode = @"
namespace RoslynSandbox
{
namespace RoslynSandbox
{
}";

            var iMehCode = @"
namespace RoslynSandbox
{
    public interface IMeh
    {
    }
}";
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(IFoo),
            typeof(FooControl));

        public IFoo Bar
        {
            get { return (IFoo)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }

        public void Meh(IMeh value)
        {
            this.SetValue(BarProperty, ↓value);
        }
    }
}";
            testCode = testCode.AssertReplace("this.SetValue(BarProperty, ↓value);", $"this.{methodName}(BarProperty, ↓value);");
            AnalyzerAssert.Diagnostics<WPF0014SetValueMustUseRegisteredType>(iFooCode, iMehCode, testCode);
        }

        [TestCase("SetValue")]
        [TestCase("SetCurrentValue")]
        public void DependencyPropertyAddOwnerMediaElementVolume(string methodName)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class MediaElementWrapper : Control
    {
        public static readonly DependencyProperty VolumeProperty = MediaElement.VolumeProperty.AddOwner(
            typeof(MediaElementWrapper),
            new FrameworkPropertyMetadata(
                MediaElement.VolumeProperty.DefaultMetadata.DefaultValue,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnVolumeChanged,
                OnVolumeCoerce));

        public MediaElementWrapper()
        {
            this.SetValue(VolumeProperty, ↓1);
        }

        public double Volume
        {
            get { return (double)this.GetValue(VolumeProperty); }
            set { this.SetValue(VolumeProperty, value); }
        }

        private static object OnVolumeCoerce(DependencyObject d, object basevalue)
        {
            return basevalue;
        }

        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";
            testCode = testCode.AssertReplace("this.SetValue(VolumeProperty, ↓1);", $"this.{methodName}(VolumeProperty, ↓1);");
            AnalyzerAssert.Diagnostics<WPF0014SetValueMustUseRegisteredType>(testCode);
        }

        [TestCase("1.0")]
        [TestCase("null")]
        [TestCase("\"abc\"")]
        public void ReadOnlyDependencyProperty(string value)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarPropertyKey, value); }
        }

        public void Meh()
        {
            this.SetValue(BarPropertyKey, ↓<value>);
        }
    }
}";
            testCode = testCode.AssertReplace("<value>", value);
            AnalyzerAssert.Diagnostics<WPF0014SetValueMustUseRegisteredType>(testCode);
        }

        [Test]
        public void AttachedProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetBar(FrameworkElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int GetBar(FrameworkElement element)
        {
            return (int)element.GetValue(BarProperty);
        }

        public static void Meh(FrameworkElement element)
        {
            element.SetValue(BarProperty, ↓1.0);
        }
    }
}";
            AnalyzerAssert.Diagnostics<WPF0014SetValueMustUseRegisteredType>(testCode);
        }

        [TestCase("SetValue")]
        [TestCase("SetCurrentValue")]
        public void TextBoxTextProperty(string setMethod)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public static class Foo
    {
        public static void Bar()
        {
            var textBox = new TextBox();
            textBox.SetValue(TextBox.TextProperty, ↓1);
        }
    }
}";
            testCode = testCode.AssertReplace("textBox.SetValue", $"textBox.{setMethod}");
            AnalyzerAssert.Diagnostics<WPF0014SetValueMustUseRegisteredType>(testCode);
        }

        [TestCase("SetValue")]
        [TestCase("SetCurrentValue")]
        public void TextElementFontSizeProperty(string setMethod)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    public static class Foo
    {
        public static void Bar()
        {
            var textBox = new TextBox();
            textBox.SetValue(TextElement.FontSizeProperty, ↓1);
        }
    }
}";
            testCode = testCode.AssertReplace("textBox.SetValue", $"textBox.{setMethod}");
            AnalyzerAssert.Diagnostics<WPF0014SetValueMustUseRegisteredType>(testCode);
        }

        [TestCase("SetValue")]
        [TestCase("SetCurrentValue")]
        public void SetCurrentValueInLambda(string setMethod)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        public void Meh()
        {
            this.Loaded += (sender, args) =>
            {
                this.SetCurrentValue(BarProperty, ↓1.0);
            };
        }
    }
}";

            testCode = testCode.AssertReplace("SetCurrentValue", setMethod);
            AnalyzerAssert.Diagnostics<WPF0014SetValueMustUseRegisteredType>(testCode);
        }
    }
}