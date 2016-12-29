namespace WpfAnalyzers.Test.PropertyChanged.WPF1015CheckIfDifferentBeforeNotifyingTests
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.PropertyChanged;

    internal class DiagnosticsWhenNoCheck : CodeFixVerifier<WPF1015CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>
    {
        [Test]
        public async Task CallsOnPropertyChanged()
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
            this.value = value;
            ↓this.OnPropertyChanged(nameof(Value));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Check if value is different before notifying.");
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
            this.OnPropertyChanged(nameof(Value));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task CallsRaisePropertyChangedWithEventArgs()
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

            var fixedCode = @"
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
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Bar)));
        }
    }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        this.PropertyChanged?.Invoke(this, e);
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task Invokes()
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
            this.value = value;
            ↓this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Check if value is different before notifying.");
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
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task WarnOnlyOnFirst()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Squared => this.Value * this.Value;

    public int Value
    {
        get
        {
            return this.value;
        }

        set
        {
            this.value = value;
            ↓this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(Squared));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Check if value is different before notifying.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Squared => this.Value * this.Value;

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
            this.OnPropertyChanged(nameof(Squared));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }
    }
}