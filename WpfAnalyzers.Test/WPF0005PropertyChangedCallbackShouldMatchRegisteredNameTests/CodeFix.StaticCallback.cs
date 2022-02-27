namespace WpfAnalyzers.Test.WPF0005PropertyChangedCallbackShouldMatchRegisteredNameTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static partial class CodeFix
{
    public static class StaticCallback
    {
        private static readonly PropertyMetadataAnalyzer Analyzer = new();
        private static readonly RenameMemberFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0005PropertyChangedCallbackShouldMatchRegisteredName);

        [Test]
        public static void Message()
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(1, ↓WrongName));

        public double Value
        {
            get { return (double)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void WrongName(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Method 'WrongName' should be named 'OnValueChanged'"), code);
        }

        [TestCase("new PropertyMetadata(default(double), ↓WrongName)",                                         "new PropertyMetadata(default(double), OnValueChanged)")]
        [TestCase("new PropertyMetadata(default(double), FooControl.↓WrongName)",                              "new PropertyMetadata(default(double), FooControl.OnValueChanged)")]
        [TestCase("new PropertyMetadata(default(double), new PropertyChangedCallback(↓WrongName))",            "new PropertyMetadata(default(double), new PropertyChangedCallback(OnValueChanged))")]
        [TestCase("new PropertyMetadata(default(double), new PropertyChangedCallback(FooControl.↓WrongName))", "new PropertyMetadata(default(double), new PropertyChangedCallback(FooControl.OnValueChanged))")]
        public static void DependencyPropertyRegister(string oldMetadata, string newMetadata)
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(default(double), ↓WrongName));

        public double Value
        {
            get { return (double)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void WrongName(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}".AssertReplace("new PropertyMetadata(default(double), ↓WrongName)", oldMetadata);

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(default(double), OnValueChanged));

        public double Value
        {
            get { return (double)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}".AssertReplace("new PropertyMetadata(default(double), OnValueChanged)", newMetadata);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("(d, e) => ((FooControl)d).↓WrongName((double)e.NewValue, (double)e.OldValue)",                              "(d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue)")]
        [TestCase("new PropertyChangedCallback((d, e) => ((FooControl)d).↓WrongName((double)e.NewValue, (double)e.OldValue))", "new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue))")]
        public static void DependencyPropertyRegisterInstanceMethod(string lambdaBefore, string lambdaAfter)
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(default(double), (d, e) => ((FooControl)d).↓WrongName((double)e.NewValue, (double)e.OldValue)));

        public double Value
        {
            get { return (double)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        protected virtual void WrongName(double newValue, double oldValue)
        {
        }
    }
}".AssertReplace("(d, e) => ((FooControl)d).↓WrongName((double)e.NewValue, (double)e.OldValue)", lambdaBefore);

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(default(double), (d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue)));

        public double Value
        {
            get { return (double)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        protected virtual void OnValueChanged(double newValue, double oldValue)
        {
        }
    }
}".AssertReplace("(d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue)", lambdaAfter);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyRegisterReadOnly()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static readonly DependencyPropertyKey ValuePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(1.0, ↓WrongName));

        public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

        public double Value
        {
            get { return (double)this.GetValue(ValueProperty); }
            set { this.SetValue(ValuePropertyKey, value); }
        }

        private static void WrongName(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static readonly DependencyPropertyKey ValuePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(1.0, OnValueChanged));

        public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

        public double Value
        {
            get { return (double)this.GetValue(ValueProperty); }
            set { this.SetValue(ValuePropertyKey, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyRegisterAttached()
        {
            var before = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(1, ↓WrongName));

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

        private static void WrongName(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";

            var after = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(1, OnBarChanged));

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyRegisterAttachedReadOnly()
        {
            var before = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int), ↓WrongName));

            public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

        private static void WrongName(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";

            var after = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int), OnBarChanged));

            public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyOverrideMetadata()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : UserControl
    {
        static FooControl()
        {
            BackgroundProperty.OverrideMetadata(typeof(FooControl),
                new FrameworkPropertyMetadata(null, ↓WrongName));
        }

        private static void WrongName(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : UserControl
    {
        static FooControl()
        {
            BackgroundProperty.OverrideMetadata(typeof(FooControl),
                new FrameworkPropertyMetadata(null, OnBackgroundChanged));
        }

        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyAddOwner()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    public class FooControl : FrameworkElement
    {
        static FooControl()
        {
            TextElement.FontSizeProperty.AddOwner(typeof(FooControl), new PropertyMetadata(12.0, ↓WrongName));
        }

        private static void WrongName(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    public class FooControl : FrameworkElement
    {
        static FooControl()
        {
            TextElement.FontSizeProperty.AddOwner(typeof(FooControl), new PropertyMetadata(12.0, OnFontSizeChanged));
        }

        private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
