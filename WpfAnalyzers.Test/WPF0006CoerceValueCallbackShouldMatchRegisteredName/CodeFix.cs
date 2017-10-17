namespace WpfAnalyzers.Test.DependencyProperties.WPF0006CoerceValueCallbackShouldMatchRegisteredName
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class CodeFix : CodeFixVerifier<WPF0006CoerceValueCallbackShouldMatchRegisteredName, RenameMethodCodeFixProvider>
    {
        [TestCase("new PropertyMetadata(1, null, ↓WrongName)")]
        [TestCase("new PropertyMetadata(1, null, new CoerceValueCallback(↓WrongName))")]
        public async Task DependencyProperty(string metadata)
        {
            var testCode = @"
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl),
        new PropertyMetadata(1, null, ↓WrongName));

    public double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }

    private static object WrongName(DependencyObject d, object baseValue)
    {
        return baseValue;
    }
}";
            testCode = testCode.AssertReplace("new PropertyMetadata(1, null, ↓WrongName)", metadata);
            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("Method 'WrongName' should be named 'CoerceValue'");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl),
        new PropertyMetadata(1, null, CoerceValue));

    public double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }

    private static object CoerceValue(DependencyObject d, object baseValue)
    {
        return baseValue;
    }
}";
            fixedCode = fixedCode.AssertReplace("new PropertyMetadata(1, null, CoerceValue)", metadata.AssertReplace("↓WrongName", "CoerceValue"));
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
        new PropertyMetadata(1.0, null, ↓WrongName));

    public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

    public double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValuePropertyKey, value); }
    }

    private static object WrongName(DependencyObject d, object baseValue)
    {
        return baseValue;
    }
}";
            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("Method 'WrongName' should be named 'CoerceValue'");
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
        new PropertyMetadata(1.0, null, CoerceValue));

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
        new PropertyMetadata(1, null, ↓WrongName));

    public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

    private static object WrongName(DependencyObject d, object baseValue)
    {
        return baseValue;
    }
}";

            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("Method 'WrongName' should be named 'CoerceBar'");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
            var fixedCode = @"
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(1, null, CoerceBar));

    public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

    private static object CoerceBar(DependencyObject d, object baseValue)
    {
        return baseValue;
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
        new PropertyMetadata(default(int), null, ↓WrongName));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

    private static object WrongName(DependencyObject d, object baseValue)
    {
        return baseValue;
    }
}";

            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("Method 'WrongName' should be named 'CoerceBar'");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;

public static class Foo
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int), null, CoerceBar));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

    private static object CoerceBar(DependencyObject d, object baseValue)
    {
        return baseValue;
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task OverrideMetadata()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : UserControl
{
    static FooControl()
    {
        BackgroundProperty.OverrideMetadata(typeof(FooControl),
            new FrameworkPropertyMetadata(null, null, ↓WrongName));
    }

    private static object WrongName(DependencyObject d, object baseValue)
    {
        return baseValue;
    }
}";

            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("Method 'WrongName' should be named 'CoerceBackground'");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : UserControl
{
    static FooControl()
    {
        BackgroundProperty.OverrideMetadata(typeof(FooControl),
            new FrameworkPropertyMetadata(null, null, CoerceBackground));
    }

    private static object CoerceBackground(DependencyObject d, object baseValue)
    {
        return baseValue;
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task AddOwner()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

public class FooControl : FrameworkElement
{
    static FooControl()
    {
        TextElement.FontSizeProperty.AddOwner(typeof(FooControl), new PropertyMetadata(12.0, null, ↓WrongName));
    }

    private static object WrongName(DependencyObject d, object baseValue)
    {
        return baseValue;
    }
}";

            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithMessage("Method 'WrongName' should be named 'CoerceFontSize'");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

public class FooControl : FrameworkElement
{
    static FooControl()
    {
        TextElement.FontSizeProperty.AddOwner(typeof(FooControl), new PropertyMetadata(12.0, null, CoerceFontSize));
    }

    private static object CoerceFontSize(DependencyObject d, object baseValue)
    {
        return baseValue;
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }
    }
}