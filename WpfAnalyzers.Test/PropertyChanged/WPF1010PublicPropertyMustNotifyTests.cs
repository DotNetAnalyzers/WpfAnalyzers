namespace WpfAnalyzers.Test.PropertyChanged
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using WpfAnalyzers.PropertyChanged;

    public class WPF1010MutablePublicPropertyMustNotifyTests : DiagnosticVerifier
    {
        [Test]
        public async Task HappyPathStruct()
        {
            var testCode = @"
public struct Foo
{
    public int Bar { get; set; }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task HappyPathGetOnly()
        {
            var testCode = @"
public class Foo
{
    public int Bar { get; } = 1;
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task HappyPathAbstract()
        {
            var testCode = @"
public abstract class Foo
{
    public abstract int Bar { get; set; }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task HappyPathStatic()
        {
            // maybe this should notify?
            var testCode = @"
public class Foo
{
    public static int Bar { get; set; }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task HappyPathInternal()
        {
            // maybe this should notify?
            var testCode = @"
internal class Foo
{
    public static int Bar { get; set; }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotifying()
        {
            var testCode = @"
using System.ComponentModel;

public class Foo
{
    public int Bar { get; set; }
}";

            var expected = this.CSharpDiagnostic().WithLocation(6, 16).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new WPF1010MutablePublicPropertyMustNotify();
        }
    }
}