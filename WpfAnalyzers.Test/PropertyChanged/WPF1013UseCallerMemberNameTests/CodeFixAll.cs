namespace WpfAnalyzers.Test.PropertyChanged.WPF1013UseCallerMemberNameTests
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.PropertyChanged;

    internal class CodeFixAll : CodeFixVerifier<WPF1013UseCallerMemberName, UseCallerMemberNameCodeFixProvider>
    {
        [Test]
        public async Task CallsOnPropertyChanged()
        {
            var testCode = @"
using System.ComponentModel;

public class ViewModel : INotifyPropertyChanged
{
    private int value1;
    private int value2;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Value1
    {
        get
        {
            return this.value1;
        }

        set
        {
            if (value == this.value1)
            {
                return;
            }

            this.value1 = value;
            this.OnPropertyChanged(nameof(Value1));
        }
    }

    public int Value2
    {
        get
        {
            return this.value2;
        }

        set
        {
            if (value == this.value2)
            {
                return;
            }

            this.value2 = value;
            this.OnPropertyChanged(nameof(Value2));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var fixedCode = @"
using System.ComponentModel;

public class ViewModel : INotifyPropertyChanged
{
    private int value1;
    private int value2;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Value1
    {
        get
        {
            return this.value1;
        }

        set
        {
            if (value == this.value1)
            {
                return;
            }

            this.value1 = value;
            this.OnPropertyChanged();
        }
    }

    public int Value2
    {
        get
        {
            return this.value2;
        }

        set
        {
            if (value == this.value2)
            {
                return;
            }

            this.value2 = value;
            this.OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAllFixAsync(testCode, fixedCode)
                      .ConfigureAwait(false);
        }
    }
}