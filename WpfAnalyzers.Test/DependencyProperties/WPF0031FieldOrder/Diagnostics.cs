namespace WpfAnalyzers.Test.DependencyProperties.WPF0031FieldOrder
{
    using System.Threading;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class Diagnostics : DiagnosticVerifier<WPF0031FieldOrder>
    {
        [Test]
        public async Task DependencyProperty()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    // referencing field initialize below
    ↓public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
        ""Bar"",
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public int Bar
    {
        get { return (int)this.GetValue(BarProperty); }
        protected set {  this.SetValue(BarPropertyKey, value); }
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("BarPropertyKey", "BarProperty");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task Attached()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    ↓public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

    public static void SetBar(DependencyObject element, int value)
    {
        element.SetValue(BarPropertyKey, value);
    }

    public static int GetBar(DependencyObject element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("BarPropertyKey", "BarProperty");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }
    }
}