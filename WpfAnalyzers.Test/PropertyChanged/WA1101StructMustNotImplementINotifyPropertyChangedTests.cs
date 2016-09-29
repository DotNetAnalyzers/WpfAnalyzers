namespace WpfAnalyzers.Test.PropertyChanged
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis.Diagnostics;

    using NUnit.Framework;

    using WpfAnalyzers.PropertyChanged;

    public class WA1101StructMustNotImplementINotifyPropertyChangedTests : DiagnosticVerifier
    {
        [Test]
        public async Task HappyPath()
        {
            var testCode = @"
public struct Foo
{
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotifying()
        {
            var testCode = @"
using System.ComponentModel;

public struct Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
}";

            var expected = this.CSharpDiagnostic().WithLocation(4, 21).WithArguments("Foo");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new WA1101StructMustNotImplementINotifyPropertyChanged();
        }
    }
}
