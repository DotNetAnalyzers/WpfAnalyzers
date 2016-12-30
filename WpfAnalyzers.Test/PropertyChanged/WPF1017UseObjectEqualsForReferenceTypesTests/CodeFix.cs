namespace WpfAnalyzers.Test.PropertyChanged.WPF1017UseObjectEqualsForReferenceTypesTests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.PropertyChanged;

    internal class CodeFix : CodeFixVerifier<WPF1017UseObjectEqualsForReferenceTypes, UseCorrectEqualityCodeFixProvider>
    {
        public static readonly EqualsItem[] EqualsSource =
            {
                new EqualsItem("object.ReferenceEquals(value, this.bar)", "Equals(value, this.bar)"),
                new EqualsItem("Object.ReferenceEquals(value, this.bar)", "Equals(value, this.bar)"),
                new EqualsItem("ReferenceEquals(value, this.bar)", "Equals(value, this.bar)"),
                new EqualsItem("ReferenceEquals(this.bar, value)", "Equals(value, this.bar)"),
                new EqualsItem("ReferenceEquals(value, bar)", "Equals(value, this.bar)"),
                new EqualsItem("ReferenceEquals(value, Bar)", "Equals(value, this.bar)"),
                new EqualsItem("ReferenceEquals(Bar, value)", "Equals(value, this.bar)"),
                new EqualsItem("Nullable.Equals(value, this.bar)", "Equals(value, this.bar)"),
                new EqualsItem("Nullable.Equals(value, this.bar)", "Equals(value, this.bar)"),
                new EqualsItem("value.Equals(this.bar)", "Equals(value, this.bar)"),
                new EqualsItem("value.Equals(bar)", "Equals(value, this.bar)"),
                new EqualsItem("this.bar.Equals(value)", "Equals(value, this.bar)"),
                new EqualsItem("bar.Equals(value)", "Equals(value, this.bar)"),
                new EqualsItem("System.Collections.Generic.EqualityComparer<Foo>.Default.Equals(value, this.bar)", null),
            };

        private static readonly string FooCode = @"
public class Foo
{
}";

        [Test]
        public async Task OperatorEquals()
        {
            var testCode = @"
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private Foo bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public Foo Bar
        {
            get { return this.bar; }
            set
            {
                ↓if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Check if value is different using object.Equals before notifying.");
            await this.VerifyCSharpDiagnosticAsync(new[] { FooCode, testCode }, expected).ConfigureAwait(false);

            var fixedCode = @"
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private Foo bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public Foo Bar
        {
            get { return this.bar; }
            set
            {
                if (Equals(value, this.bar))
                {
                    return;
                }

                this.bar = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }";
            await this.VerifyCSharpFixAsync(new[] { FooCode, testCode }, new[] { FooCode, fixedCode }).ConfigureAwait(false);
        }

        [Test]
        public async Task OperatorNotEquals()
        {
            var testCode = @"
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private Foo bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public Foo Bar
        {
            get { return this.bar; }
            set
            {
                ↓if (value != this.bar)
                {
                    this.bar = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
                }
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Check if value is different using object.Equals before notifying.");
            await this.VerifyCSharpDiagnosticAsync(new[] { FooCode, testCode }, expected).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(new[] { FooCode, testCode }, new[] { FooCode, testCode }).ConfigureAwait(false);
        }

        [TestCaseSource(nameof(EqualsSource))]
        public async Task Check(EqualsItem check)
        {
            var testCode = @"
using System;
using System.ComponentModel;

public class ViewModel : INotifyPropertyChanged
{
    private Foo bar;

    public event PropertyChangedEventHandler PropertyChanged;

    public Foo Bar
    {
        get { return this.bar; }
        set
        {
            ↓if (Equals(value, this.bar))
            {
                return;
            }

            this.bar = value;
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
        }
    }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        this.PropertyChanged?.Invoke(this, e);
    }
}";
            testCode = testCode.AssertReplace("Equals(value, this.bar)", check.Call);
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Check if value is different using object.Equals before notifying.");
            await this.VerifyCSharpDiagnosticAsync(new[] { FooCode, testCode }, expected).ConfigureAwait(false);

            var fixedCode = @"
using System;
using System.ComponentModel;

public class ViewModel : INotifyPropertyChanged
{
    private Foo bar;

    public event PropertyChangedEventHandler PropertyChanged;

    public Foo Bar
    {
        get { return this.bar; }
        set
        {
            if (Equals(value, this.bar))
            {
                return;
            }

            this.bar = value;
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
        }
    }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        this.PropertyChanged?.Invoke(this, e);
    }
}";
            fixedCode = check.FixedCall == null
                            ? fixedCode.AssertReplace("Equals(value, this.bar)", check.Call)
                            : fixedCode.AssertReplace("Equals(value, this.bar)", check.FixedCall);
            await this.VerifyCSharpFixAsync(new[] { FooCode, testCode }, new[] { FooCode, fixedCode }, allowNewCompilerDiagnostics: true).ConfigureAwait(false);
        }

        [TestCaseSource(nameof(EqualsSource))]
        public async Task NegatedCheck(EqualsItem check)
        {
            var testCode = @"
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private Foo bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public Foo Bar
        {
            get { return this.bar; }
            set
            {
                ↓if (!Equals(value, this.bar))
                {
                    this.bar = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
                }
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }";
            testCode = testCode.AssertReplace("Equals(value, this.bar)", check.Call);
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Check if value is different using object.Equals before notifying.");
            await this.VerifyCSharpDiagnosticAsync(new[] { FooCode, testCode }, expected).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(new[] { FooCode, testCode }, new[] { FooCode, testCode }).ConfigureAwait(false);
        }

        protected override IEnumerable<string> GetDisabledDiagnostics()
        {
            return new[] { WPF1016UseReferenceEquals.DiagnosticId };
        }

        public class EqualsItem
        {
            public EqualsItem(string call, string fixedCall)
            {
                this.Call = call;
                this.FixedCall = fixedCall;
            }

            internal string Call { get; }

            internal string FixedCall { get; }

            public override string ToString() => this.Call;
        }
    }
}