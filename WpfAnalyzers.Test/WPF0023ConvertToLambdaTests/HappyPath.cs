namespace WpfAnalyzers.Test.WPF0023ConvertToLambdaTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    [TestFixture(typeof(RegistrationAnalyzer))]
    [TestFixture(typeof(PropertyMetadataAnalyzer))]
    public class HappyPath<T>
        where T : DiagnosticAnalyzer, new()
    {
        private static readonly DiagnosticAnalyzer Analyzer = new T();

        [Test]
        public void DependencyPropertyRegisterPropertyChangedCallbackLambdaCallingInstanceMethod()
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
            typeof(string),
            typeof(FooControl),
            new PropertyMetadata(
                default(string),
                (d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue)));

        public string Value
        {
            get => (string)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void OnValueChanged(object oldValue, object newValue)
        {
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, ExpectedDiagnostic.Create("WPF0023"), testCode);
        }

        [TestCase("new PropertyMetadata(OnBarChanged)")]
        [TestCase("new PropertyMetadata(new PropertyChangedCallback(OnBarChanged))")]
        [TestCase("new PropertyMetadata(default(int), OnBarChanged)")]
        [TestCase("new PropertyMetadata(default(int), new PropertyChangedCallback(OnBarChanged))")]
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
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
            var oldValue = (int)d.OldValue;
            var newValue = (int)d.NewValue;
        }

        private static object CoerceBar(DependencyObject d, object baseValue)
        {
            var o = baseValue;
            return o;
        }
    }
}";
            testCode = testCode.AssertReplace("new PropertyMetadata(default(int), OnBarChanged)", metadata);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void DependencyPropertyRegisterWithAllCallbacksMoreThanOneStatement()
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
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int), OnValueChanged, CoerceValue),
            ValidateValue);

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
            var oldValue = (int)d.OldValue;
            var newValue = (int)d.NewValue;
        }

        private static object CoerceValue(DependencyObject d, object basevalue)
        {
            var control = (FooControl)d;
            var value = (int)basevalue;
            return value;
        }

        private static bool ValidateValue(object basevalue)
        {
            var notNull = ((int)basevalue) != null;
            return notNull;
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void DependencyPropertyRegisterOnPropertyChangedIf()
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
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int), OnValueChanged));

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FooControl control)
            {
            }
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}
