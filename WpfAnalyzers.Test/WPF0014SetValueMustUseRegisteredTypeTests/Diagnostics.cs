namespace WpfAnalyzers.Test.WPF0014SetValueMustUseRegisteredTypeTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Diagnostics
{
    private static readonly SetValueAnalyzer Analyzer = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0014SetValueMustUseRegisteredType);

    [Test]
    public static void Message()
    {
        var code = @"
namespace N
{
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

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("SetValue must use registered type int"), code);
    }

    [TestCase("SetValue(BarProperty, ↓1.0)")]
    [TestCase("SetCurrentValue(BarProperty, ↓1.0)")]
    [TestCase("this.SetValue(BarProperty, ↓1.0)")]
    [TestCase("this.SetCurrentValue(BarProperty, ↓1.0)")]
    [TestCase("SetValue(BarProperty, ↓null)")]
    [TestCase("SetCurrentValue(BarProperty, ↓null)")]
    [TestCase("SetValue(BarProperty, ↓\"abc\")")]
    [TestCase("SetCurrentValue(BarProperty, ↓\"abc\")")]
    public static void DependencyProperty(string setCall)
    {
        var code = @"
namespace N
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
}".AssertReplace("this.SetValue(BarProperty, ↓1)", setCall);

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }

    [TestCase("this.SetValue(BarProperty, ↓1.0);")]
    [TestCase("this.SetCurrentValue(BarProperty, ↓1.0);")]
    public static void DependencyPropertyGeneric(string setValueCall)
    {
        var fooControlGeneric = @"
namespace N
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

        var code = @"
namespace N
{
    public class FooControl : FooControl<int>
    {
        public void Meh()
        {
            this.SetValue(BarProperty, ↓1.0);
        }
    }
}".AssertReplace("this.SetValue(BarProperty, ↓1.0)", setValueCall);

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, fooControlGeneric, code);
    }

    [TestCase("this.SetValue(BarProperty, ↓1);")]
    [TestCase("this.SetCurrentValue(BarProperty, ↓1);")]
    public static void DependencyPropertyAddOwner(string setValueCall)
    {
        var fooCode = @"
namespace N
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
namespace N
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

        private static object OnVolumeCoerce(DependencyObject d, object baseValue)
        {
            return baseValue;
        }

        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";

        var code = @"
namespace N
{
    public partial class FooControl
    {
        public FooControl()
        {
            this.SetValue(BarProperty, ↓1);
        }
    }
}".AssertReplace("this.SetValue(BarProperty, ↓1);", setValueCall);
        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, fooCode, fooControlPart1, code);
    }

    [Test]
    public static void AddOwnerTextElementFontSize()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Documents;

    public class FooControl : FrameworkElement
    {
        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(FooControl));

        public double FontSize
        {
            get => (double)this.GetValue(FontSizeProperty);
            set => this.SetValue(FontSizeProperty, value);
        }

        public void Update(int i) => this.SetValue(FontSizeProperty, ↓i);
    }
}";

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }

    [Test]
    public static void AddOwnerBorderBorderThicknessProperty()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : FrameworkElement
    {
        public static readonly DependencyProperty BorderThicknessProperty = Border.BorderThicknessProperty.AddOwner(typeof(FooControl));

        public Thickness BorderThickness
        {
            get => (Thickness)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        public void Update(int i) => this.SetValue(BorderThicknessProperty, ↓i);
    }
}";

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }

    [TestCase("SetValue")]
    [TestCase("SetCurrentValue")]
    public static void DependencyPropertyOfInterfaceType(string methodName)
    {
        var iFooCode = @"
namespace N
{
    public interface IFoo
    {
    }
}";

        var iMehCode = @"
namespace N
{
    public interface IMeh
    {
    }
}";
        var code = @"
namespace N
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
}".AssertReplace("this.SetValue(BarProperty, ↓value);", $"this.{methodName}(BarProperty, ↓value);");

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, iFooCode, iMehCode, code);
    }

    [TestCase("SetValue")]
    [TestCase("SetCurrentValue")]
    public static void DependencyPropertyAddOwnerMediaElementVolume(string methodName)
    {
        var code = @"
namespace N
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

        private static object OnVolumeCoerce(DependencyObject d, object baseValue)
        {
            return baseValue;
        }

        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}".AssertReplace("this.SetValue(VolumeProperty, ↓1);", $"this.{methodName}(VolumeProperty, ↓1);");

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }

    [TestCase("1.0")]
    [TestCase("null")]
    [TestCase("\"abc\"")]
    public static void ReadOnlyDependencyProperty(string value)
    {
        var code = @"
namespace N
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
}".AssertReplace("<value>", value);

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }

    [Test]
    public static void DependencyPropertyRegisterAttached()
    {
        var code = @"
namespace N
{
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
        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }

    [TestCase("SetValue")]
    [TestCase("SetCurrentValue")]
    public static void TextBoxTextProperty(string setMethod)
    {
        var code = @"
namespace N
{
    using System.Windows.Controls;

    public static class Foo
    {
        public static void Bar()
        {
            var textBox = new TextBox();
            textBox.SetValue(TextBox.TextProperty, ↓1);
        }
    }
}".AssertReplace("textBox.SetValue", $"textBox.{setMethod}");

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }

    [TestCase("SetValue")]
    [TestCase("SetCurrentValue")]
    public static void TextElementFontSizeProperty(string setMethod)
    {
        var code = @"
namespace N
{
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
}".AssertReplace("textBox.SetValue", $"textBox.{setMethod}");

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }

    [TestCase("SetValue")]
    [TestCase("SetCurrentValue")]
    public static void SetCurrentValueInLambda(string setMethod)
    {
        var code = @"
namespace N
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
}".AssertReplace("SetCurrentValue", setMethod);

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }
}