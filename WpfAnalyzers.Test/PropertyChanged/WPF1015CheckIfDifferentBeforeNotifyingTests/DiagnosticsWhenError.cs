namespace WpfAnalyzers.Test.PropertyChanged.WPF1015CheckIfDifferentBeforeNotifyingTests
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.PropertyChanged;

    internal class DiagnosticsWhenError : DiagnosticVerifier<WPF1015CheckIfDifferentBeforeNotifying>
    {
        public static readonly EqualsItem[] EqualsSource =
        {
            new EqualsItem("string", "Equals(value, this.bar)"),
            new EqualsItem("string", "Equals(this.bar, value)"),
            new EqualsItem("string", "Equals(value, bar)"),
            new EqualsItem("string", "Equals(value, Bar)"),
            new EqualsItem("string", "Equals(Bar, value)"),
            new EqualsItem("string", "Nullable.Equals(value, this.bar)"),
            new EqualsItem("int?", "Nullable.Equals(value, this.bar)"),
            new EqualsItem("string", "value.Equals(this.bar)"),
            new EqualsItem("string", "value.Equals(bar)"),
            new EqualsItem("string", "this.bar.Equals(value)"),
            new EqualsItem("string", "bar.Equals(value)"),
            //new EqualsItem("string", "string.Equals(value, this.bar, StringComparison.OrdinalIgnoreCase)"),
            new EqualsItem("string", "System.Collections.Generic.EqualityComparer<string>.Default.Equals(value, this.bar)"),
            new EqualsItem("string", "ReferenceEquals(value, this.bar)"),
        };

        [Test]
        public async Task OperatorNotEquals()
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
                if (value != this.bar)
                {
                    return;
                }

                this.bar = value;
                ↓this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Check if value is different before notifying.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        [Test]
        public async Task OperatorEquals()
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
                if (value == this.bar)
                {
                    this.bar = value;
                    ↓this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
                }
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Check if value is different before notifying.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        [TestCaseSource(nameof(EqualsSource))]
        public async Task Check(EqualsItem check)
        {
            var testCode = @"
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private string bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Bar
        {
            get { return this.bar; }
            set
            {
                if (Equals(value, this.bar))
                {
                    this.bar = value;
                    ↓this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
                }
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }";
            testCode = testCode.AssertReplace("Equals(value, this.bar)", check.Call).AssertReplace("string", check.Type);
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Check if value is different before notifying.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
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
        private string bar;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Bar
        {
            get { return this.bar; }
            set
            {
                if (!Equals(value, this.bar))
                {
                    return;
                }

                this.bar = value;
                ↓this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }";
            testCode = testCode.AssertReplace("Equals(value, this.bar)", check.Call).AssertReplace("string", check.Type);
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Check if value is different before notifying.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        public class EqualsItem
        {
            internal readonly string Type;
            internal readonly string Call;

            public EqualsItem(string type, string call)
            {
                this.Type = type;
                this.Call = call;
            }

            public override string ToString()
            {
                return $"{nameof(this.Type)}: {this.Type}, {nameof(this.Call)}: {this.Call}";
            }
        }
    }
}