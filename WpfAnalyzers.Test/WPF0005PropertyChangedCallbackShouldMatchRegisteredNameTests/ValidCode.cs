namespace WpfAnalyzers.Test.WPF0005PropertyChangedCallbackShouldMatchRegisteredNameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    [TestFixture(typeof(CallbackAnalyzer))]
    [TestFixture(typeof(PropertyMetadataAnalyzer))]
    internal class ValidCode<T>
        where T : DiagnosticAnalyzer, new()
    {
        private static readonly DiagnosticAnalyzer Analyzer = new T();

        [Test]
        public void DependencyPropertyRegisterNoMetadata()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl));

        public double Value
        {
            get { return (double)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("new PropertyMetadata(OnBarChanged)")]
        [TestCase("new PropertyMetadata(new PropertyChangedCallback(OnBarChanged))")]
        [TestCase("new PropertyMetadata(default(int), OnBarChanged)")]
        [TestCase("new PropertyMetadata(default(int), new PropertyChangedCallback(OnBarChanged))")]
        [TestCase("new PropertyMetadata(default(int), (d, e) => ((FooControl)d).OnBarChanged(e.OldValue, e.NewValue))")]
        [TestCase("new PropertyMetadata(default(int), new PropertyChangedCallback((d, e) => ((FooControl)d).OnBarChanged(e.OldValue, e.NewValue)))")]
        [TestCase("new PropertyMetadata((o, e) => { })")]
        [TestCase("new FrameworkPropertyMetadata((o, e) => { })")]
        [TestCase("new FrameworkPropertyMetadata(OnBarChanged)")]
        [TestCase("new FrameworkPropertyMetadata(OnBarChanged, CoerceBar)")]
        public void DependencyPropertyRegisterWithMetadata(string metadata)
        {
            var testCode = @"
namespace RoslynSandbox
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
            get => (int)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }

        /// <summary>This method is invoked when the <see cref=""BarProperty""/> changes.</summary>
        /// <param name=""oldValue"">The old value of <see cref=""BarProperty""/>.</param>
        /// <param name=""newValue"">The new value of <see cref=""BarProperty""/>.</param>
        protected virtual void OnBarChanged(object oldValue, object newValue)
        {
        }

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
            control.OnBarChanged(e.OldValue, e.NewValue);
        }

        private static object CoerceBar(DependencyObject d, object baseValue)
        {
            if (baseValue is int i &&
                i < 0)
            {
                return 0;
            }

            return baseValue;
        }
    }
}";
            testCode = testCode.AssertReplace("new PropertyMetadata(default(int), OnBarChanged)", metadata);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void DependencyPropertyRegisterReadOnly()
        {
            var testCode = @"
namespace RoslynSandbox
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void DependencyPropertyRegisterAttached()
        {
            var testCode = @"
namespace RoslynSandbox
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
            // nop
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void DependencyPropertyRegisterAttachedReadOnly()
        {
            var testCode = @"
namespace RoslynSandbox
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void DependencyPropertyOverrideMetadata()
        {
            var testCode = @"
namespace RoslynSandbox
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void DependencyPropertyAddOwner()
        {
            var testCode = @"
namespace RoslynSandbox
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void LambdaCallingMethodCalledInOtherPlaces()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(
                default(int),
                (d, e) => Meh(d)));

        public int Bar
        {
            get => (int) this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Meh(null);
        }

        private static void Meh(DependencyObject o)
        {
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}
