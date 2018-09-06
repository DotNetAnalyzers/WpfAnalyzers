namespace WpfAnalyzers.Test.WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredNameTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly RegistrationAnalyzer Analyzer = new RegistrationAnalyzer();

        [TestCase("CommonValidation.ValidateDoubleIsGreaterThanZero")]
        [TestCase("o => CommonValidation.ValidateDoubleIsGreaterThanZero(o)")]
        [TestCase("new ValidateValueCallback(CommonValidation.ValidateDoubleIsGreaterThanZero))")]
        [TestCase("new ValidateValueCallback(o => CommonValidation.ValidateDoubleIsGreaterThanZero(o))")]
        public void WhenValidationMethodInHelperClass(string callback)
        {
            var validationCode = @"
namespace RoslynSandbox
{
    internal static class CommonValidation
    {
        public static bool ValidateDoubleIsGreaterThanZero(object value)
        {
            if (value is double d)
            {
                return d > 0;
            }

            return false;
        }
    }
}";
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
            typeof(double),
            typeof(FooControl),
            new FrameworkPropertyMetadata(1d),
            CommonValidation.ValidateDoubleIsGreaterThanZero);

        /// <summary>Identifies the <see cref=""Baz""/> dependency property.</summary>
        public static readonly DependencyProperty BazProperty = DependencyProperty.Register(
            nameof(Baz),
            typeof(double),
            typeof(FooControl),
            new FrameworkPropertyMetadata(1d),
            CommonValidation.ValidateDoubleIsGreaterThanZero);

        public double Bar
        {
            get => (double)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }

        public double Baz
        {
            get => (double)this.GetValue(BazProperty);
            set => this.SetValue(BazProperty, value);
        }
    }
}";
            testCode = testCode.AssertReplace("CommonValidation.ValidateDoubleIsGreaterThanZero", callback);
            AnalyzerAssert.Valid(Analyzer, validationCode, testCode);
        }

        [Test]
        public void WhenValidationMethodIsUsedMoreThanOnce()
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
            typeof(double),
            typeof(FooControl),
            new FrameworkPropertyMetadata(1d),
            ValidateDoubleIsGreaterThanZero);

        /// <summary>Identifies the <see cref=""Baz""/> dependency property.</summary>
        public static readonly DependencyProperty BazProperty = DependencyProperty.Register(
            nameof(Baz),
            typeof(double),
            typeof(FooControl),
            new FrameworkPropertyMetadata(1d),
            ValidateDoubleIsGreaterThanZero);

        public double Bar
        {
            get => (double)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }

        public double Baz
        {
            get => (double)this.GetValue(BazProperty);
            set => this.SetValue(BazProperty, value);
        }

        private static bool ValidateDoubleIsGreaterThanZero(object value)
        {
            if (value is double d)
            {
                return d > 0;
            }

            return false;
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void DependencyPropertyNoCallback()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

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

        [TestCase("ValidateValue);")]
        [TestCase("o => ValidateValue(o));")]
        [TestCase("new ValidateValueCallback(ValidateValue));")]
        [TestCase("o => (int)o >= 0);")]
        public void DependencyPropertyWithCallback(string metadata)
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
            new PropertyMetadata(default(int)),
            ValidateValue);

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static bool ValidateValue(object value)
        {
            if (value is int i)
            {
                return i > 0;
            }

            return false;
        }
    }
}";
            testCode = testCode.AssertReplace("ValidateValue);", metadata);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void ReadOnlyDependencyProperty()
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
            new PropertyMetadata(1.0, null, CoerceValue),
            ValidateValue);

        public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

        public double Value
        {
            get { return (double)this.GetValue(ValueProperty); }
            set { this.SetValue(ValuePropertyKey, value); }
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            return baseValue;
        }

        private static bool ValidateValue(object value)
        {
            if (value is int i)
            {
                return i > 0;
            }

            return false;
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
            new PropertyMetadata(default(int), null, CoerceBar),
            ValidateBar);

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

        private static object CoerceBar(DependencyObject d, object baseValue)
        {
            return baseValue;
        }

        private static bool ValidateBar(object value)
        {
            if (value is int i)
            {
                return i > 0;
            }

            return false;
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
            new PropertyMetadata(default(int), OnBarChanged, CoerceBar),
            ValidateBar);

            public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }

        private static object CoerceBar(DependencyObject d, object baseValue)
        {
            return baseValue;
        }

        private static bool ValidateBar(object value)
        {
            if (value is int i)
            {
                return i > 0;
            }

            return false;
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenUsingMethodInOtherClass()
        {
            var commonValidation = @"
namespace RoslynSandbox
{
    internal static class CommonValidation
    {
        public static bool ValidateDoubleIsGreaterThanZero(object value)
        {
            if (value is double d)
            {
                return d > 0;
            }

            return false;
        }
    }
}";
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
            typeof(double),
            typeof(FooControl),
            new FrameworkPropertyMetadata(1d),
            CommonValidation.ValidateDoubleIsGreaterThanZero);

        public double Bar
        {
            get => (double)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, commonValidation, testCode);
        }
    }
}
