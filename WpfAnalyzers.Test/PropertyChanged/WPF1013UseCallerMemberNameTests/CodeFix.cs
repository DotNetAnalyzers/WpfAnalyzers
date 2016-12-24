namespace WpfAnalyzers.Test.PropertyChanged.WPF1013UseCallerMemberNameTests
{
    using System.Threading.Tasks;
    using NUnit.Framework;
    using WpfAnalyzers.PropertyChanged;

    internal class CodeFix : CodeFixVerifier<WPF1013UseCallerMemberName, UseCallerMemberNameCodeFixProvider>
    {
        [Test]
        public async Task CallsOnPropertyChanged()
        {
            var testCode = @"
using System.ComponentModel;

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
            this.OnPropertyChanged(nameof(Value));
        }
    }

    protected virtual void OnPropertyChanged(↓string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Use [CallerMemberName]");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

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
            this.OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [TestCase(@"""Value""")]
        [TestCase(@"nameof(Value)")]
        [TestCase(@"nameof(this.Value)")]
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

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Use [CallerMemberName]");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
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
            this.OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }
    }
}