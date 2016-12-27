namespace WpfAnalyzers.Test.PropertyChanged.WPF1012NotifyWhenPropertyChangesTests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis.CSharp;

    using NUnit.Framework;
    using WpfAnalyzers.PropertyChanged;

    internal class CodeFix : CodeFixVerifier<WPF1012NotifyWhenPropertyChanges, NotifyPropertyChangedCodeFixProvider>
    {
        [TestCase("this.value = value;")]
        [TestCase("this.value += value;")]
        [TestCase("this.value -= value;")]
        [TestCase("this.value *= value;")]
        [TestCase("this.value /= value;")]
        [TestCase("this.value %= value;")]
        [TestCase("this.value++;")]
        [TestCase("this.value--;")]
        [TestCase("--this.value;")]
        [TestCase("++this.value;")]
        public async Task IntFieldUpdatedInMethod(string update)
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Value => this.value;

    public void Update(int value)
    {
        ↓this.value = value;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            testCode = testCode.AssertReplace("this.value = value;", update);
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Value");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Value => this.value;

    public void Update(int value)
    {
        this.value = value;
        this.OnPropertyChanged(nameof(this.Value));
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            fixedCode = fixedCode.AssertReplace("this.value = value;", update);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [TestCase("_value = value;")]
        [TestCase("_value += value;")]
        [TestCase("_value -= value;")]
        [TestCase("_value *= value;")]
        [TestCase("_value /= value;")]
        [TestCase("_value %= value;")]
        [TestCase("_value++;")]
        [TestCase("_value--;")]
        [TestCase("--_value;")]
        [TestCase("++_value;")]
        public async Task IntFieldUpdatedInMethodUnderscoreNames(string update)
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private static readonly int Meh = 2;
    private int _value;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Value => _value;

    public void Update(int value)
    {
        ↓_value = value;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            testCode = testCode.AssertReplace("_value = value;", update);
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Value");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private static readonly int Meh = 2;
    private int _value;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Value => _value;

    public void Update(int value)
    {
        _value = value;
        OnPropertyChanged(nameof(Value));
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            fixedCode = fixedCode.AssertReplace("_value = value;", update);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [TestCase("this.value |= value;")]
        [TestCase("this.value ^= value;")]
        [TestCase("this.value &= value;")]
        public async Task BoolFieldUpdatedInMethod(string update)
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private bool value;

    public event PropertyChangedEventHandler PropertyChanged;

    public bool Value => this.value;

    public void Update(bool value)
    {
        ↓this.value = value;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            testCode = testCode.AssertReplace("this.value = value;", update);
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Value");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private bool value;

    public event PropertyChangedEventHandler PropertyChanged;

    public bool Value => this.value;

    public void Update(bool value)
    {
        this.value = value;
        this.OnPropertyChanged(nameof(this.Value));
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            fixedCode = fixedCode.AssertReplace("this.value = value;", update);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task WhenUsingPropertiesExpressionBody()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string FullName => $""{this.FirstName} {this.LastName}"";

    public string FirstName
    {
        get
        {
            return this.firstName;
        }

        set
        {
            if (value == this.firstName)
            {
                return;
            }

            ↓this.firstName = value;
            this.OnPropertyChanged();
        }
    }

    public string LastName
    {
        get
        {
            return this.lastName;
        }

        set
        {
            if (value == this.lastName)
            {
                return;
            }

            this.lastName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("FullName");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string FullName => $""{this.FirstName} {this.LastName}"";

    public string FirstName
    {
        get
        {
            return this.firstName;
        }

        set
        {
            if (value == this.firstName)
            {
                return;
            }

            this.firstName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
        }
    }

    public string LastName
    {
        get
        {
            return this.lastName;
        }

        set
        {
            if (value == this.lastName)
            {
                return;
            }

            this.lastName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
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

        [Test]
        public async Task WhenUsingPropertiesExpressionBodyTernarySimple()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string name;

    public event PropertyChangedEventHandler PropertyChanged;

    public string CalculatedName => this.name == null ? ""Missing"" : this.name;

    public string Name
    {
        get
        {
            return this.name;
        }

        set
        {
            if (value == this.name)
            {
                return;
            }

            ↓this.name = value;
            this.OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("CalculatedName");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string name;

    public event PropertyChangedEventHandler PropertyChanged;

    public string CalculatedName => this.name == null ? ""Missing"" : this.name;

    public string Name
    {
        get
        {
            return this.name;
        }

        set
        {
            if (value == this.name)
            {
                return;
            }

            this.name = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.CalculatedName));
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

        [Test]
        public async Task WhenUsingPropertiesExpressionBodyNested()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Greeting => $""Hello {this.FullName}"";

    public string FullName
    {
        get
        {
            return $""{this.FirstName} {this.LastName}"";
        }
    }

    public string FirstName
    {
        get
        {
            return this.firstName;
        }

        set
        {
            if (value == this.firstName)
            {
                return;
            }

            ↓this.firstName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
        }
    }

    public string LastName
    {
        get
        {
            return this.lastName;
        }

        set
        {
            if (value == this.lastName)
            {
                return;
            }

            this.lastName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
            this.OnPropertyChanged(nameof(this.Greeting));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Greeting");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Greeting => $""Hello {this.FullName}"";

    public string FullName
    {
        get
        {
            return $""{this.FirstName} {this.LastName}"";
        }
    }

    public string FirstName
    {
        get
        {
            return this.firstName;
        }

        set
        {
            if (value == this.firstName)
            {
                return;
            }

            this.firstName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
            this.OnPropertyChanged(nameof(this.Greeting));
        }
    }

    public string LastName
    {
        get
        {
            return this.lastName;
        }

        set
        {
            if (value == this.lastName)
            {
                return;
            }

            this.lastName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
            this.OnPropertyChanged(nameof(this.Greeting));
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

        [Test]
        public async Task WhenUsingPropertiesExpressionBodyNestedSimple()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Greeting => $""Hello {this.FullName}"";

    public string FullName
    {
        get
        {
            return $""{this.FirstName}"";
        }
    }

    public string FirstName
    {
        get
        {
            return this.firstName;
        }

        set
        {
            if (value == this.firstName)
            {
                return;
            }

            ↓this.firstName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Greeting");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Greeting => $""Hello {this.FullName}"";

    public string FullName
    {
        get
        {
            return $""{this.FirstName}"";
        }
    }

    public string FirstName
    {
        get
        {
            return this.firstName;
        }

        set
        {
            if (value == this.firstName)
            {
                return;
            }

            this.firstName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
            this.OnPropertyChanged(nameof(this.Greeting));
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

        [Test]
        public async Task WhenUsingPropertiesExpressionCallingMethod()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Greeting => CreateGreeting();

    public string FullName
    {
        get
        {
            return $""{this.FirstName} {this.LastName}"";
        }
    }

    public string FirstName
    {
        get
        {
            return this.firstName;
        }

        set
        {
            if (value == this.firstName)
            {
                return;
            }

            ↓this.firstName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
        }
    }

    public string LastName
    {
        get
        {
            return this.lastName;
        }

        set
        {
            if (value == this.lastName)
            {
                return;
            }

            this.lastName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
            this.OnPropertyChanged(nameof(this.Greeting));
        }
    }

    public string CreateGreeting() => $""Hello {this.FullName}"";

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Greeting");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Greeting => CreateGreeting();

    public string FullName
    {
        get
        {
            return $""{this.FirstName} {this.LastName}"";
        }
    }

    public string FirstName
    {
        get
        {
            return this.firstName;
        }

        set
        {
            if (value == this.firstName)
            {
                return;
            }

            this.firstName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
            this.OnPropertyChanged(nameof(this.Greeting));
        }
    }

    public string LastName
    {
        get
        {
            return this.lastName;
        }

        set
        {
            if (value == this.lastName)
            {
                return;
            }

            this.lastName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
            this.OnPropertyChanged(nameof(this.Greeting));
        }
    }

    public string CreateGreeting() => $""Hello {this.FullName}"";

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task WhenUsingPropertiesExpressionBodyTernary()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string FullName => this.FirstName == null ? this.LastName : $""{this.FirstName} {this.LastName}"";

    public string FirstName
    {
        get
        {
            return this.firstName;
        }

        set
        {
            if (value == this.firstName)
            {
                return;
            }

            ↓this.firstName = value;
            this.OnPropertyChanged();
        }
    }

    public string LastName
    {
        get
        {
            return this.lastName;
        }

        set
        {
            if (value == this.lastName)
            {
                return;
            }

            this.lastName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("FullName");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string FullName => this.FirstName == null ? this.LastName : $""{this.FirstName} {this.LastName}"";

    public string FirstName
    {
        get
        {
            return this.firstName;
        }

        set
        {
            if (value == this.firstName)
            {
                return;
            }

            this.firstName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
        }
    }

    public string LastName
    {
        get
        {
            return this.lastName;
        }

        set
        {
            if (value == this.lastName)
            {
                return;
            }

            this.lastName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
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

        [Test]
        public async Task WhenUsingPropertiesStatementBody()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string FullName
    {
        get
        {
            return $""{this.FirstName} {this.LastName}"";
        }
    }

    public string FirstName
    {
        get
        {
            return this.firstName;
        }

        set
        {
            if (value == this.firstName)
            {
                return;
            }

            ↓this.firstName = value;
            this.OnPropertyChanged();
        }
    }

    public string LastName
    {
        get
        {
            return this.lastName;
        }

        set
        {
            if (value == this.lastName)
            {
                return;
            }

            this.lastName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("FullName");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string FullName
    {
        get
        {
            return $""{this.FirstName} {this.LastName}"";
        }
    }

    public string FirstName
    {
        get
        {
            return this.firstName;
        }

        set
        {
            if (value == this.firstName)
            {
                return;
            }

            this.firstName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
        }
    }

    public string LastName
    {
        get
        {
            return this.lastName;
        }

        set
        {
            if (value == this.lastName)
            {
                return;
            }

            this.lastName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
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

        [Test]
        public async Task WhenUsingPropertiesStatementBodyUnderscoreNames()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string _firstName;
    private string _lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string FullName
    {
        get
        {
            return $""{FirstName}{LastName}"";
        }
    }

    public string FirstName
    {
        get
        {
            return _firstName;
        }

        set
        {
            if (value == _firstName)
            {
                return;
            }

            ↓_firstName = value;
            OnPropertyChanged();
        }
    }

    public string LastName
    {
        get
        {
            return _lastName;
        }

        set
        {
            if (value == _lastName)
            {
                return;
            }

            _lastName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FullName));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("FullName");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string _firstName;
    private string _lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string FullName
    {
        get
        {
            return $""{FirstName}{LastName}"";
        }
    }

    public string FirstName
    {
        get
        {
            return _firstName;
        }

        set
        {
            if (value == _firstName)
            {
                return;
            }

            _firstName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FullName));
        }
    }

    public string LastName
    {
        get
        {
            return _lastName;
        }

        set
        {
            if (value == _lastName)
            {
                return;
            }

            _lastName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FullName));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task WhenUsingBackingFieldsExpressionBody()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string FullName => $""{this.firstName} {this.lastName}"";

    public string FirstName
    {
        get
        {
            return this.firstName;
        }

        set
        {
            if (value == this.firstName)
            {
                return;
            }

            ↓this.firstName = value;
            this.OnPropertyChanged();
        }
    }

    public string LastName
    {
        get
        {
            return this.lastName;
        }

        set
        {
            if (value == this.lastName)
            {
                return;
            }

            this.lastName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("FullName");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string FullName => $""{this.firstName} {this.lastName}"";

    public string FirstName
    {
        get
        {
            return this.firstName;
        }

        set
        {
            if (value == this.firstName)
            {
                return;
            }

            this.firstName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
        }
    }

    public string LastName
    {
        get
        {
            return this.lastName;
        }

        set
        {
            if (value == this.lastName)
            {
                return;
            }

            this.lastName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
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

        [Test]
        public async Task WhenUsingBackingFieldsStatementBody()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string FullName
    {
        get
        {
            return $""{this.firstName} {this.lastName}"";
        }
    }

    public string FirstName
    {
        get
        {
            return this.firstName;
        }

        set
        {
            if (value == this.firstName)
            {
                return;
            }

            ↓this.firstName = value;
            this.OnPropertyChanged();
        }
    }

    public string LastName
    {
        get
        {
            return this.lastName;
        }

        set
        {
            if (value == this.lastName)
            {
                return;
            }

            this.lastName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("FullName");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string firstName;
    private string lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string FullName
    {
        get
        {
            return $""{this.firstName} {this.lastName}"";
        }
    }

    public string FirstName
    {
        get
        {
            return this.firstName;
        }

        set
        {
            if (value == this.firstName)
            {
                return;
            }

            this.firstName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
        }
    }

    public string LastName
    {
        get
        {
            return this.lastName;
        }

        set
        {
            if (value == this.lastName)
            {
                return;
            }

            this.lastName = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.FullName));
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

        [Test]
        public async Task WhenUsingBackingFieldsStatementBodyWithSwitch()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string name;

    public event PropertyChangedEventHandler PropertyChanged;

    public string CalculatedName
    {
        get
        {
            switch (this.name)
            {
                case ""Meh"":
                    return ""heM"";
                default:
                    return this.name;
            }
        }
    }

    public string Name
    {
        get
        {
            return this.name;
        }

        set
        {
            if (value == this.name)
            {
                return;
            }

            ↓this.name = value;
            this.OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("CalculatedName");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string name;

    public event PropertyChangedEventHandler PropertyChanged;

    public string CalculatedName
    {
        get
        {
            switch (this.name)
            {
                case ""Meh"":
                    return ""heM"";
                default:
                    return this.name;
            }
        }
    }

    public string Name
    {
        get
        {
            return this.name;
        }

        set
        {
            if (value == this.name)
            {
                return;
            }

            this.name = value;
            this.OnPropertyChanged();
            this.OnPropertyChanged(nameof(this.CalculatedName));
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

        [Test]
        public async Task WhenUsingBackingFieldsStatementBodyUnderscoreNames()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string _firstName;
    private string _lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string FullName
    {
        get
        {
            return $""{_firstName} {_lastName}"";
        }
    }

    public string FirstName
    {
        get
        {
            return _firstName;
        }

        set
        {
            if (value == _firstName)
            {
                return;
            }

            ↓_firstName = value;
            OnPropertyChanged();
        }
    }

    public string LastName
    {
        get
        {
            return _lastName;
        }

        set
        {
            if (value == _lastName)
            {
                return;
            }

            _lastName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FullName));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("FullName");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string _firstName;
    private string _lastName;

    public event PropertyChangedEventHandler PropertyChanged;

    public string FullName
    {
        get
        {
            return $""{_firstName} {_lastName}"";
        }
    }

    public string FirstName
    {
        get
        {
            return _firstName;
        }

        set
        {
            if (value == _firstName)
            {
                return;
            }

            _firstName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FullName));
        }
    }

    public string LastName
    {
        get
        {
            return _lastName;
        }

        set
        {
            if (value == _lastName)
            {
                return;
            }

            _lastName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FullName));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task FieldUpdatedInMethodWithInvokerInBaseClass()
        {
            var viewModelBaseCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : ViewModelBase
{
    private string text;

    public string Text => this.text;

    public void Update(string text)
    {
        ↓this.text = text;
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Text");
            await this.VerifyCSharpDiagnosticAsync(new[] { viewModelBaseCode, testCode }, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : ViewModelBase
{
    private string text;

    public string Text => this.text;

    public void Update(string text)
    {
        this.text = text;
        this.OnPropertyChanged(nameof(this.Text));
    }
}";

            await this.VerifyCSharpFixAsync(new[] { viewModelBaseCode, testCode }, new[] { viewModelBaseCode, fixedCode }, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task InLambda1()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string name;

    public ViewModel()
    {
        this.PropertyChanged += (o, e) => ↓this.name = this.name + ""meh"";
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name => this.name;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Name");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string name;

    public ViewModel()
    {
        this.PropertyChanged += (o, e) =>
        {
            this.name = this.name + ""meh"";
            this.OnPropertyChanged(nameof(this.Name));
        };
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name => this.name;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task InLambda2()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string name;

    public ViewModel()
    {
        this.PropertyChanged += (o, e) =>
            {
                ↓this.name = this.name + ""meh"";
            };
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name => this.name;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Name");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string name;

    public ViewModel()
    {
        this.PropertyChanged += (o, e) =>
            {
                this.name = this.name + ""meh"";
                this.OnPropertyChanged(nameof(this.Name));
            };
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name => this.name;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task AssigningFieldsInGetter()
        {
            foreach (var name in Enum.GetNames(typeof(SyntaxKind)).Where(x => x.Contains("Expression")))
            {
                Console.WriteLine(name);
            }

            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string name;
    private int getCount;

    public event PropertyChangedEventHandler PropertyChanged;

    public int GetCount => this.getCount;

    public string Name
    {
        get
        {
            ↓this.getCount++;
            return this.name;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("GetCount");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string name;
    private int getCount;

    public event PropertyChangedEventHandler PropertyChanged;

    public int GetCount => this.getCount;

    public string Name
    {
        get
        {
            this.getCount++;
            this.OnPropertyChanged(nameof(this.GetCount));
            return this.name;
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