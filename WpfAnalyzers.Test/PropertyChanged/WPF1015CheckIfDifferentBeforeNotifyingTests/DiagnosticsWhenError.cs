namespace WpfAnalyzers.Test.PropertyChanged.WPF1015CheckIfDifferentBeforeNotifyingTests
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.PropertyChanged;

    internal class DiagnosticsWhenError : DiagnosticVerifier<WPF1015CheckIfDifferentBeforeNotifying>
    {
        public static readonly string[] EqualsCalls =
            {
                "Equals(value, this.bar)",
                "Equals(value, bar)",
                "Equals(value, Bar)",
                "Nullable.Equals(value, this.bar)",
                "value.Equals(this.bar)",
                "value.Equals(bar)",
                "this.bar.Equals(value)",
                "bar.Equals(value)",
                "string.Equals(value, this.bar, StringComparison.OrdinalIgnoreCase)",
                "System.Collections.Generic.EqualityComparer<string>.Default.Equals(value, this.bar)",
                "ReferenceEquals(value, this.bar)",
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

        [TestCaseSource(nameof(EqualsCalls))]
        public async Task Check(string check)
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
            testCode = testCode.AssertReplace("Equals(value, this.bar)", check);
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Check if value is different before notifying.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        [TestCaseSource(nameof(EqualsCalls))]
        public async Task NegatedCheck(string check)
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
            testCode = testCode.AssertReplace("Equals(value, this.bar)", check);
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Check if value is different before notifying.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }
    }
}