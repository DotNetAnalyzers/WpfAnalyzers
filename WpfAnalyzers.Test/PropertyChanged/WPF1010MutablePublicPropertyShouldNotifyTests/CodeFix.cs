namespace WpfAnalyzers.Test.PropertyChanged.WPF1010MutablePublicPropertyShouldNotifyTests
{
    using System.Threading;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.PropertyChanged;

    internal class CodeFix : CodeFixVerifier<WPF1010MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>
    {
        [TestCase("public int Bar { get; set; }")]
        [TestCase("public int Bar { get; private set; }")]
        public async Task WhenNotNotifyingAutoProperty(string property)
        {
            var testCode = @"
public class Foo
{
    ↓public int Bar { get; set; }
}";

            testCode = testCode.AssertReplace("public int Bar { get; set; }", property);
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotNotifyingWithBackingField()
        {
            var testCode = @"
public class Foo
{
    private int value;

    ↓public int Value
    {
        get
        {
            return this.value;
        }
        private set
        {
            this.value = value;
        }
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Value");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }
    }
}