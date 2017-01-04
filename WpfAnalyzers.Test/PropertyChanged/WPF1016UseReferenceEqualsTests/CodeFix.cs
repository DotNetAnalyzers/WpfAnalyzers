namespace WpfAnalyzers.Test.PropertyChanged.WPF1016UseReferenceEqualsTests
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.PropertyChanged;

    internal class CodeFix : CodeFixVerifier<WPF1016UseReferenceEquals, UseCorrectEqualityCodeFixProvider>
    {
        public static readonly EqualsItem[] EqualsSource =
            {
                new EqualsItem("Equals(value, this.bar)", "ReferenceEquals(value, this.bar)"),
                new EqualsItem("Equals(this.bar, value)", "ReferenceEquals(value, this.bar)"),
                new EqualsItem("Equals(value, bar)", "ReferenceEquals(value, this.bar)"),
                new EqualsItem("Equals(value, Bar)", "ReferenceEquals(value, this.bar)"),
                new EqualsItem("Equals(Bar, value)", "ReferenceEquals(value, this.bar)"),
                new EqualsItem("Nullable.Equals(value, this.bar)", "ReferenceEquals(value, this.bar)"),
                new EqualsItem("Nullable.Equals(value, this.bar)", "ReferenceEquals(value, this.bar)"),
                new EqualsItem("value.Equals(this.bar)", "ReferenceEquals(value, this.bar)"),
                new EqualsItem("value.Equals(bar)", "ReferenceEquals(value, this.bar)"),
                new EqualsItem("this.bar.Equals(value)", "ReferenceEquals(value, this.bar)"),
                new EqualsItem("bar.Equals(value)", "ReferenceEquals(value, this.bar)"),
                new EqualsItem("System.Collections.Generic.EqualityComparer<Foo>.Default.Equals(value, this.bar)", null),
            };

        private static readonly string FooCode = @"
public class Foo
{
}";

        [Test]
        public async Task ConstrainedGeneric()
        {
            var testCode = @"
using System.ComponentModel;

public class ViewModel<T> : INotifyPropertyChanged
    where T : class
{
    private T bar;
    public event PropertyChangedEventHandler PropertyChanged;

    public T Bar
    {
        get { return this.bar; }
        set
        {
            ↓if (Equals(value, this.bar))
            {
                return;
            }

            this.bar = value;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Bar)));
        }
    }
}";
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Check if value is different using ReferenceEquals before notifying.");
            await this.VerifyCSharpDiagnosticAsync(new[] { FooCode, testCode }, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class ViewModel<T> : INotifyPropertyChanged
    where T : class
{
    private T bar;
    public event PropertyChangedEventHandler PropertyChanged;

    public T Bar
    {
        get { return this.bar; }
        set
        {
            if (ReferenceEquals(value, this.bar))
            {
                return;
            }

            this.bar = value;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Bar)));
        }
    }
}";
            await this.VerifyCSharpFixAsync(new[] { FooCode, testCode }, new[] { FooCode, fixedCode }).ConfigureAwait(false);
        }

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

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Check if value is different using ReferenceEquals before notifying.");
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
                if (ReferenceEquals(value, this.bar))
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

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Check if value is different using ReferenceEquals before notifying.");
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
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Check if value is different using ReferenceEquals before notifying.");
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
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Check if value is different using ReferenceEquals before notifying.");
            await this.VerifyCSharpDiagnosticAsync(new[] { FooCode, testCode }, expected).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(new[] { FooCode, testCode }, new[] { FooCode, testCode }).ConfigureAwait(false);
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