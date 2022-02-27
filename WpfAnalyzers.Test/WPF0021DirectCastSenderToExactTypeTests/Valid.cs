namespace WpfAnalyzers.Test.WPF0021DirectCastSenderToExactTypeTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly CallbackAnalyzer Analyzer = new();

    [Test]
    public static void DependencyPropertyRegisterNoMetadata()
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
            typeof(int),
            typeof(FooControl));

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [TestCase("new PropertyMetadata(OnBarChanged)")]
    [TestCase("new PropertyMetadata(new PropertyChangedCallback(OnBarChanged))")]
    [TestCase("new PropertyMetadata(default(int), OnBarChanged)")]
    [TestCase("new PropertyMetadata(default(int), new PropertyChangedCallback(OnBarChanged))")]
    [TestCase("new PropertyMetadata((o, e) => { })")]
    [TestCase("new FrameworkPropertyMetadata((o, e) => { })")]
    [TestCase("new FrameworkPropertyMetadata(OnBarChanged)")]
    [TestCase("new FrameworkPropertyMetadata(OnBarChanged, CoerceBar)")]
    public static void DependencyPropertyRegisterWithMetadata(string metadata)
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int), OnBarChanged));

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
            var oldValue = (int)e.OldValue;
            var newValue = (int)e.NewValue;
        }

        private static object CoerceBar(DependencyObject d, object baseValue)
        {
            return baseValue;
        }
    }
}".AssertReplace("new PropertyMetadata(default(int), OnBarChanged)", metadata);

        RoslynAssert.Valid(Analyzer, code);
    }

    [TestCase("FooControl")]
    public static void DependencyPropertyRegisterDirectCast(string type)
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int), OnBarChanged, CoerceBar));

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
            var oldValue = (int)e.OldValue;
            var newValue = (int)e.NewValue;
        }

        private static object CoerceBar(DependencyObject d, object baseValue)
        {
            var control = (FooControl)d;
            var value = (int)baseValue;
            return value;
        }
    }
}".AssertReplace("(FooControl)", $"({type})");

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void DependencyPropertyRegisterReadOnly()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static readonly DependencyPropertyKey ValuePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Value),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(1.0, OnValueChanged));

        public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValuePropertyKey, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
            var oldValue = (int)e.OldValue;
            var newValue = (int)e.NewValue;
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
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
            new PropertyMetadata(default(int), OnBarChanged));

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FrameworkElement)d;
            var oldValue = (int)e.OldValue;
            var newValue = (int)e.NewValue;
        }
    }
}";

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void DependencyPropertyRegisterAttachedReadOnly()
    {
        var code = @"
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
            var control = (FrameworkElement)d;
            var oldValue = (int)e.OldValue;
            var newValue = (int)e.NewValue;
        }
    }
}";

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void DependencyPropertyOverrideMetadata()
    {
        var code = @"
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
            var control = (FooControl)d;
            var oldValue = (System.Windows.Media.Brush)e.OldValue;
            var newValue = (System.Windows.Media.Brush)e.NewValue;
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void DependencyPropertyAddOwner()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Documents;

    public class FooControl : FrameworkElement
    {
        static FooControl()
        {
            TextElement.FontSizeProperty.AddOwner(typeof(FooControl), new PropertyMetadata(12.0, OnFontSizeChanged));
        }

        private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
            var oldValue = (double)e.OldValue;
            var newValue = (double)e.NewValue;
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }
}