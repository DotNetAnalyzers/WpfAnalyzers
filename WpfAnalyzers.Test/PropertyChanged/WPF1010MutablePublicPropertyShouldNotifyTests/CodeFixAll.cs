namespace WpfAnalyzers.Test.PropertyChanged.WPF1010MutablePublicPropertyShouldNotifyTests
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.PropertyChanged;

    internal class CodeFixAll : CodeFixVerifier<WPF1010MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>
    {
        [Test]
        public async Task AutoPropertiesCallerMemberNameNameUnderscoreNames()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar1 { get; set; }

    public int Bar2 { get; set; }

    public int Bar3 { get; set; }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int _bar1;
    private int _bar2;
    private int _bar3;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar1
    {
        get
        {
            return _bar1;
        }

        set
        {
            if (value == _bar1)
            {
                return;
            }

            _bar1 = value;
            OnPropertyChanged();
        }
    }

    public int Bar2
    {
        get
        {
            return _bar2;
        }

        set
        {
            if (value == _bar2)
            {
                return;
            }

            _bar2 = value;
            OnPropertyChanged();
        }
    }

    public int Bar3
    {
        get
        {
            return _bar3;
        }

        set
        {
            if (value == _bar3)
            {
                return;
            }

            _bar3 = value;
            OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAllFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertiesCallerMemberNameNameUnderscoreNamesWithExistingNotifying1()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int _bar1;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar1
    {
        get
        {
            return _bar1;
        }

        set
        {
            if (value == _bar1)
            {
                return;
            }

            _bar1 = value;
            OnPropertyChanged();
        }
    }

    public int Bar2 { get; set; }

    public int Bar3 { get; set; }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int _bar1;
    private int _bar2;
    private int _bar3;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar1
    {
        get
        {
            return _bar1;
        }

        set
        {
            if (value == _bar1)
            {
                return;
            }

            _bar1 = value;
            OnPropertyChanged();
        }
    }

    public int Bar2
    {
        get
        {
            return _bar2;
        }

        set
        {
            if (value == _bar2)
            {
                return;
            }

            _bar2 = value;
            OnPropertyChanged();
        }
    }

    public int Bar3
    {
        get
        {
            return _bar3;
        }

        set
        {
            if (value == _bar3)
            {
                return;
            }

            _bar3 = value;
            OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAllFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertiesCallerMemberNameNameUnderscoreNamesWithExistingNotifying2()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int _bar2;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar1 { get; set; }

    public int Bar2
    {
        get
        {
            return _bar2;
        }

        set
        {
            if (value == _bar2)
            {
                return;
            }

            _bar2 = value;
            OnPropertyChanged();
        }
    }

    public int Bar3 { get; set; }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int _bar1;
    private int _bar2;
    private int _bar3;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar1
    {
        get
        {
            return _bar1;
        }

        set
        {
            if (value == _bar1)
            {
                return;
            }

            _bar1 = value;
            OnPropertyChanged();
        }
    }

    public int Bar2
    {
        get
        {
            return _bar2;
        }

        set
        {
            if (value == _bar2)
            {
                return;
            }

            _bar2 = value;
            OnPropertyChanged();
        }
    }

    public int Bar3
    {
        get
        {
            return _bar3;
        }

        set
        {
            if (value == _bar3)
            {
                return;
            }

            _bar3 = value;
            OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAllFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertiesCallerMemberNameNameUnderscoreNamesWithExistingNotifying3()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int _bar3;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar1 { get; set; }

    public int Bar2 { get; set; }

    public int Bar3
    {
        get
        {
            return _bar3;
        }

        set
        {
            if (value == _bar3)
            {
                return;
            }

            _bar3 = value;
            OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int _bar1;
    private int _bar2;
    private int _bar3;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar1
    {
        get
        {
            return _bar1;
        }

        set
        {
            if (value == _bar1)
            {
                return;
            }

            _bar1 = value;
            OnPropertyChanged();
        }
    }

    public int Bar2
    {
        get
        {
            return _bar2;
        }

        set
        {
            if (value == _bar2)
            {
                return;
            }

            _bar2 = value;
            OnPropertyChanged();
        }
    }

    public int Bar3
    {
        get
        {
            return _bar3;
        }

        set
        {
            if (value == _bar3)
            {
                return;
            }

            _bar3 = value;
            OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAllFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertiesCallerMemberNameNameUnderscoreNamesTwoClassesInDocument()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo1 : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar1 { get; set; }

    public int Bar2 { get; set; }

    public int Bar3 { get; set; }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class Foo2 : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar1 { get; set; }

    public int Bar2 { get; set; }

    public int Bar3 { get; set; }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo1 : INotifyPropertyChanged
{
    private int _bar1;
    private int _bar2;
    private int _bar3;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar1
    {
        get
        {
            return _bar1;
        }

        set
        {
            if (value == _bar1)
            {
                return;
            }

            _bar1 = value;
            OnPropertyChanged();
        }
    }

    public int Bar2
    {
        get
        {
            return _bar2;
        }

        set
        {
            if (value == _bar2)
            {
                return;
            }

            _bar2 = value;
            OnPropertyChanged();
        }
    }

    public int Bar3
    {
        get
        {
            return _bar3;
        }

        set
        {
            if (value == _bar3)
            {
                return;
            }

            _bar3 = value;
            OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class Foo2 : INotifyPropertyChanged
{
    private int _bar1;
    private int _bar2;
    private int _bar3;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar1
    {
        get
        {
            return _bar1;
        }

        set
        {
            if (value == _bar1)
            {
                return;
            }

            _bar1 = value;
            OnPropertyChanged();
        }
    }

    public int Bar2
    {
        get
        {
            return _bar2;
        }

        set
        {
            if (value == _bar2)
            {
                return;
            }

            _bar2 = value;
            OnPropertyChanged();
        }
    }

    public int Bar3
    {
        get
        {
            return _bar3;
        }

        set
        {
            if (value == _bar3)
            {
                return;
            }

            _bar3 = value;
            OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAllFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }
    }
}