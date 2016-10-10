namespace WpfAnalyzers.Test.DependencyProperties.WPF0032ClrPropertyGetAndSetSameDependencyProperty
{
    using System.Threading;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class Diagnostics : DiagnosticVerifier<WPF0032ClrPropertyGetAndSetSameDependencyProperty>
    {
        [Test]
        public async Task DependencyProperty()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
        nameof(Bar),
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty OtherProperty = DependencyProperty.Register(
        ""Other"",
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public int Bar
    {
        get { return (int)this.GetValue(BarProperty); }
        set { this.SetValue(OtherProperty, value); }
    }
}";
            var expected = this.CSharpDiagnostic().WithLocation(19, 5).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }
    }
}