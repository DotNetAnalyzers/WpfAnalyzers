namespace WpfAnalyzers.Test.PropertyChanged
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;
    using WpfAnalyzers.PropertyChanged;

    public class WPF1010MutablePublicPropertyShouldNotifyTests : DiagnosticVerifier
    {
        [Test]
        public async Task HappyPathCallsRaisePropertyChanged()
        {
            var testCode = @"
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar) return;
                this.bar = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task HappyPathCallsInvoke()
        {
            var testCode = @"
    using System.ComponentModel;

    public class ViewModel : INotifyPropertyChanged
    {
        private int bar;
        public event PropertyChangedEventHandler PropertyChanged;

        public int Bar
        {
            get { return this.bar; }
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Bar)));
            }
        }
    }";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoreStruct()
        {
            var testCode = @"
public struct Foo
{
    public int Bar { get; set; }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoreGetOnly()
        {
            var testCode = @"
public class Foo
{
    public int Bar { get; } = 1;
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoreAbstract()
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
        public async Task IgnoreInternalClass()
        {
            // maybe this should notify?
            var testCode = @"
internal class Foo
{
    public int Bar { get; set; }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoreInternalProperty()
        {
            // maybe this should notify?
            var testCode = @"
public class Foo
{
    internal int Bar { get; set; }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [TestCase("public int Bar { get; set; }")]
        [TestCase("public int Bar { get; private set; }")]
        public async Task WhenNotNotifyingAutoProperty(string property)
        {
            var testCode = @"
public class Foo
{
    public int Bar { get; set; }
}";

            testCode = testCode.AssertReplace("public int Bar { get; set; }", property);
            var expected = this.CSharpDiagnostic().WithLocation(4, 16).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotNotifyingWithBackingField()
        {
            var testCode = @"
public class Foo
{
    private int value;

    public int Value
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

            var expected = this.CSharpDiagnostic().WithLocation(6, 16).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new WPF1010MutablePublicPropertyShouldNotify();
        }
    }
}