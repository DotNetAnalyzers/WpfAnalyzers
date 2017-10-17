namespace WpfAnalyzers.Test.WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName
{
    using System.Threading.Tasks;
    using NUnit.Framework;
    using WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName = WpfAnalyzers.WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName;

    internal class CodeFix : CodeFixVerifier<WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName, RenameMethodCodeFixProvider>
    {
        [TestCase("↓WrongName")]
        [TestCase("new ValidateValueCallback(↓WrongName)")]
        public async Task DependencyPropertyWithCallback(string callback)
        {
            var testCode = @"
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
}";
            testCode = testCode.AssertReplace("↓WrongName", callback);
            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("Method 'WrongName' should be named 'ValueValidateValue'");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)),
        ValueValidateValue);

    public int Value
    {
        get { return (int)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }

    private static bool ValueValidateValue(object value)
    {
        return (int)value >= 0;
    }
}";
            fixedCode = fixedCode.AssertReplace("ValueValidateValue);", callback.AssertReplace("↓WrongName", "ValueValidateValue") + ");");
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task ReadOnlyDependencyProperty()
        {
            var testCode = @"
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
}";
            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("Method 'WrongName' should be named 'ValueValidateValue'");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    private static readonly DependencyPropertyKey ValuePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Value),
        typeof(double),
        typeof(FooControl),
        new PropertyMetadata(1.0, null, CoerceValue),
        ValueValidateValue);

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

    private static bool ValueValidateValue(object value)
    {
        return (int)value >= 0;
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task AttachedProperty()
        {
            var testCode = @"
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
}";

            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("Method 'WrongName' should be named 'BarValidateValue'");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
            var fixedCode = @"
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(1, null, null),
        BarValidateValue);

    public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

    private static bool BarValidateValue(object value)
    {
        return (int)value >= 0;
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task ReadOnlyAttachedProperty()
        {
            var testCode = @"
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
}";

            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("Method 'WrongName' should be named 'BarValidateValue'");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;

public static class Foo
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int), null, CoerceBar),
        BarValidateValue);

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

    private static object CoerceBar(DependencyObject d, object baseValue)
    {
        return baseValue;
    }

    private static bool BarValidateValue(object value)
    {
        return (int)value >= 0;
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }
    }
}