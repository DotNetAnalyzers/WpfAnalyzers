namespace WpfAnalyzers.Test
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis.Diagnostics;

    using NUnit.Framework;

    internal abstract class HappyPathVerifier<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        private static readonly DiagnosticVerifier<TAnalyzer> DiagnosticVerifier = new DiagnosticVerifier<TAnalyzer>();

        [Test]
        public void IdMatches()
        {
            foreach (var diagnosticAnalyzer in DiagnosticVerifier.GetCSharpDiagnosticAnalyzers())
            {
                StringAssert.StartsWith(diagnosticAnalyzer.SupportedDiagnostics.Single().Id, diagnosticAnalyzer.GetType().Name);
                StringAssert.Contains(diagnosticAnalyzer.GetType().Name, this.GetType().FullName, "Name of test class does not match analyzer name.");
            }
        }

        protected async Task VerifyHappyPathAsync(string testCode)
        {
            await DiagnosticVerifier.VerifyCSharpDiagnosticAsync(testCode, Test.DiagnosticVerifier.EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        protected async Task VerifyHappyPathAsync(string testCode1, string testCode2)
        {
            await DiagnosticVerifier.VerifyCSharpDiagnosticAsync(new[] { testCode1, testCode2 }, Test.DiagnosticVerifier.EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        protected async Task VerifyHappyPathAsync(string[] testCode)
        {
            await DiagnosticVerifier.VerifyCSharpDiagnosticAsync(testCode, Test.DiagnosticVerifier.EmptyDiagnosticResults).ConfigureAwait(false);
        }

        protected async Task VerifyHappyPathAsync(string[] testCode, string[] filenames)
        {
            await DiagnosticVerifier.VerifyCSharpDiagnosticAsync(testCode, Test.DiagnosticVerifier.EmptyDiagnosticResults, CancellationToken.None, filenames).ConfigureAwait(false);
        }
    }
}