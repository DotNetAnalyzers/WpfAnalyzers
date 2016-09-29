namespace WpfAnalyzers.Test.DependencyProperties
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis.Diagnostics;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    public class WA1231ClrAccessorsForAttachedPropertyMustMatchRegisteredTypeTests : DiagnosticVerifier
    {
        [Test]
        public async Task HappyPathFormatted()
        {
            var testCode = @"
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
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task HappyPathWithThis()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

    public static void SetBar(this FrameworkElement element, int value)
    {
        element.SetValue(BarProperty, value);
    }

    public static int GetBar(this FrameworkElement element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task HappyPathReadonly()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotMatchingSetMethod()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

    public static void SetBar(FrameworkElement element, double value)
    {
        element.SetValue(BarProperty, value);
    }

    public static int GetBar(FrameworkElement element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";
            var expected = this.CSharpDiagnostic().WithLocation(12, 57).WithArguments("SetBar", "public static void SetBar(System.Windows.FrameworkElement element, int value)");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotMatchingSetMethodWithThis()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

    public static void SetBar(this FrameworkElement element, double value)
    {
        element.SetValue(BarProperty, value);
    }

    public static int GetBar(this FrameworkElement element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";

            var expected = this.CSharpDiagnostic().WithLocation(12, 62).WithArguments("SetBar", "public static void SetBar(this System.Windows.FrameworkElement element, int value)");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotMatchingSetMethodReadonly()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public static void SetBar(this FrameworkElement element, double value) => element.SetValue(BarPropertyKey, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);
}";

            var expected = this.CSharpDiagnostic().WithLocation(14, 62).WithArguments("SetBar", "public static void SetBar(this System.Windows.FrameworkElement element, int value)");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotMatchingGetMethod()
        {
            var testCode = @"
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

    public static double GetBar(FrameworkElement element)
    {
        return (double)element.GetValue(BarProperty);
    }
}";
            var expected = this.CSharpDiagnostic().WithLocation(17, 19).WithArguments("GetBar", "public static int GetBar(System.Windows.FrameworkElement element)");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotMatchingGetMethodWithThis()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

    public static void SetBar(this FrameworkElement element, int value)
    {
        element.SetValue(BarProperty, value);
    }

    public static double GetBar(this FrameworkElement element)
    {
        return (double)element.GetValue(BarProperty);
    }
}";

            var expected = this.CSharpDiagnostic().WithLocation(17, 19).WithArguments("GetBar", "public static int GetBar(this System.Windows.FrameworkElement element)");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new WA1231ClrAccessorsForAttachedPropertyMustMatchRegisteredType();
        }
    }
}