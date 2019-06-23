namespace WpfAnalyzers.Test.WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredNameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new RegistrationAnalyzer();
        private static readonly CodeFixProvider Fix = new RenameMemberFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName.Descriptor);

        [Test]
        public void Message()
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
            ↓WrongName);

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

        private static bool WrongName(object value)
        {
            return (int)value >= 0;
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Method 'WrongName' should be named 'ValidateValue'"), testCode);
        }

        [TestCase("↓WrongName", "ValidateValue")]
        [TestCase("x => ↓WrongName(x)", "x => ValidateValue(x)")]
        [TestCase("x => FooControl.↓WrongName(x)", "x => FooControl.ValidateValue(x)")]
        [TestCase("FooControl.↓WrongName", "FooControl.ValidateValue")]
        [TestCase("new ValidateValueCallback(↓WrongName)", "new ValidateValueCallback(ValidateValue)")]
        [TestCase("new ValidateValueCallback(FooControl.↓WrongName)", "new ValidateValueCallback(FooControl.ValidateValue)")]
        [TestCase("new ValidateValueCallback(x => ↓WrongName(x))", "new ValidateValueCallback(x => ValidateValue(x))")]
        [TestCase("new ValidateValueCallback(x => FooControl.↓WrongName(x))", "new ValidateValueCallback(x => FooControl.ValidateValue(x))")]
        public void DependencyPropertyWithCallback(string callback, string expected)
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
            ↓WrongName);

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static bool WrongName(object value)
        {
            return (int)value >= 0;
        }
    }
}".AssertReplace("↓WrongName", callback);

            var fixedCode = @"
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
            return (int)value >= 0;
        }
    }
}".AssertReplace("ValidateValue);", $"{expected});");

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
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
            ↓WrongName);

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

        private static bool WrongName(object value)
        {
            return (int)value >= 0;
        }
    }
}";

            var fixedCode = @"
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
            return (int)value >= 0;
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
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
            new PropertyMetadata(1, null, null),
            ↓WrongName);

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

        private static bool WrongName(object value)
        {
            return (int)value >= 0;
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(1, null, null),
            ValidateBar);

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

        private static bool ValidateBar(object value)
        {
            return (int)value >= 0;
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
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
            new PropertyMetadata(default(int), null, CoerceBar),
            ↓WrongName);

            public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

        private static object CoerceBar(DependencyObject d, object baseValue)
        {
            return baseValue;
        }

        private static bool WrongName(object value)
        {
            return (int)value >= 0;
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int), null, CoerceBar),
            ValidateBar);

            public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

        private static object CoerceBar(DependencyObject d, object baseValue)
        {
            return baseValue;
        }

        private static bool ValidateBar(object value)
        {
            return (int)value >= 0;
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenCallbackMatchesOtherProperty()
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
            ValidateBar);

        /// <summary>Identifies the <see cref=""Baz""/> dependency property.</summary>
        public static readonly DependencyProperty BazProperty = DependencyProperty.Register(
            nameof(Baz),
            typeof(double),
            typeof(FooControl),
            new FrameworkPropertyMetadata(1d),
            ↓ValidateBar);

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

        private static bool ValidateBar(object value)
        {
            if (value is double d)
            {
                return d > 0;
            }

            return false;
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Method 'ValidateBar' should be named 'ValidateBaz'"), testCode);
            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, testCode);
        }

        [Test]
        public void WhenCallbackMatchesOtherPropertyParts()
        {
            var part1 = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(double),
            typeof(FooControl),
            new FrameworkPropertyMetadata(1d),
            ValidateBar);

        public double Bar
        {
            get => (double)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }

        private static bool ValidateBar(object value)
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

    public partial class FooControl
    {
        /// <summary>Identifies the <see cref=""Baz""/> dependency property.</summary>
        public static readonly DependencyProperty BazProperty = DependencyProperty.Register(
            nameof(Baz),
            typeof(double),
            typeof(FooControl),
            new FrameworkPropertyMetadata(1d),
            ↓ValidateBar);

        public double Baz
        {
            get => (double)this.GetValue(BazProperty);
            set => this.SetValue(BazProperty, value);
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Method 'ValidateBar' should be named 'ValidateBaz'"), part1, testCode);
            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, part1, testCode);
        }
    }
}
