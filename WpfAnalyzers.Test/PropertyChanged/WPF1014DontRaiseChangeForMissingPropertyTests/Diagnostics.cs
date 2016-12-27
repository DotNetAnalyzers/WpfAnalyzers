namespace WpfAnalyzers.Test.PropertyChanged.WPF1014DontRaiseChangeForMissingPropertyTests
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.PropertyChanged;

    internal class Diagnostics : DiagnosticVerifier<WPF1014DontRaiseChangeForMissingProperty>
    {
        [TestCase(@"""Missing""")]
        [TestCase(@"nameof(PropertyChanged)")]
        [TestCase(@"nameof(this.PropertyChanged)")]
        public async Task CallsOnPropertyChangedWithExplicitNameOfCaller(string propertyName)
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Value
    {
        get
        {
            return this.value;
        }

        set
        {
            if (value == this.value)
            {
                return;
            }

            this.value = value;
            this.OnPropertyChanged(↓nameof(Value));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            testCode = testCode.AssertReplace(@"nameof(Value)", propertyName);

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Don't raise PropertyChanged for missing property.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        [Explicit]
        [TestCase(@"""Missing""")]
        [TestCase(@"nameof(PropertyChanged)")]
        [TestCase(@"nameof(this.PropertyChanged)")]
        public async Task CallsRaisePropertyChangedWithEventArgs(string propertyName)
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
                this.OnPropertyChanged(new PropertyChangedEventArgs(↓nameof(Bar)));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }
    }";

            testCode = testCode.AssertReplace(@"nameof(Bar)", propertyName);
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Don't raise PropertyChanged for missing property.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        [TestCase(@"""Missing""")]
        [TestCase(@"nameof(PropertyChanged)")]
        [TestCase(@"nameof(this.PropertyChanged)")]
        public async Task Invokes(string propertyName)
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Value
    {
        get
        {
            return this.value;
        }

        set
        {
            if (value == this.value)
            {
                return;
            }

            this.value = value;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(↓nameof(Value)));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            testCode = testCode.AssertReplace(@"nameof(Value)", propertyName);
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Don't raise PropertyChanged for missing property.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }
    }
}