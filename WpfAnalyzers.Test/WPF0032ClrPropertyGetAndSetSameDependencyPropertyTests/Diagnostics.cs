namespace WpfAnalyzers.Test.WPF0032ClrPropertyGetAndSetSameDependencyPropertyTests
{
    using System.Threading.Tasks;
    using NUnit.Framework;
    using WPF0032ClrPropertyGetAndSetSameDependencyProperty = WpfAnalyzers.WPF0032ClrPropertyGetAndSetSameDependencyProperty;

    internal class Diagnostics : DiagnosticVerifier<WPF0032ClrPropertyGetAndSetSameDependencyProperty>
    {
        [Test]
        public async Task DependencyProperty()
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
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty OtherProperty = DependencyProperty.Register(
            ""Other"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        ↓public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(OtherProperty, value); }
        }
    }
}";
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }
    }
}