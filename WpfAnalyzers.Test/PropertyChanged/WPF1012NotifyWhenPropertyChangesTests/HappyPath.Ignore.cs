namespace WpfAnalyzers.Test.PropertyChanged.WPF1012NotifyWhenPropertyChangesTests
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    internal partial class HappyPath
    {
        internal class Ignore : NestedHappyPathVerifier<HappyPath>
        {
            [Test]
            public async Task IgnoringLazy1()
            {
                var commandCode = @"
namespace RoslynSandBox
{
    using System;
    using System.Windows.Input;

    public class DelegateCommand : ICommand
    {
        public DelegateCommand(Func<object, bool> func)
        {
            throw new NotImplementedException();
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}";
                var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private bool foo;
        private DelegateCommand fooCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand FooCommand
        {
            get
            {
                if (this.fooCommand != null)
                {
                    return this.fooCommand;
                }

                this.fooCommand = new DelegateCommand(param => this.Foo = true);
                return this.fooCommand;
            }
        }

        public bool Foo
        {
            get
            {
                return this.foo;
            }

            set
            {
                if (this.foo != value)
                {
                    this.foo = value;
                    this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

                await this.VerifyHappyPathAsync(commandCode, testCode).ConfigureAwait(false);
            }

            [Test]
            public async Task IgnoringLazy2()
            {
                var commandCode = @"
namespace RoslynSandBox
{
    using System;
    using System.Windows.Input;

    public class DelegateCommand : ICommand
    {
        public DelegateCommand(Func<object, bool> func)
        {
            throw new NotImplementedException();
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}";
                var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private bool foo;
        private DelegateCommand fooCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand FooCommand
        {
            get
            {
                if (this.fooCommand == null)
                {
                    this.fooCommand = new DelegateCommand(param => this.Foo = true);
                }

                return this.fooCommand;
            }
        }

        public bool Foo
        {
            get
            {
                return this.foo;
            }

            set
            {
                if (this.foo != value)
                {
                    this.foo = value;
                    this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

                await this.VerifyHappyPathAsync(commandCode, testCode).ConfigureAwait(false);
            }

            [Test]
            public async Task IgnoringLazyNullCoalesce()
            {
                var commandCode = @"
namespace RoslynSandBox
{
    using System;
    using System.Windows.Input;

    public class DelegateCommand : ICommand
    {
        public DelegateCommand(Func<object, bool> func)
        {
            throw new NotImplementedException();
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}";
                var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private bool foo;
        private DelegateCommand fooCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand FooCommand
        {
            get
            {
                return this.fooCommand ?? (this.fooCommand = new DelegateCommand(param => this.Foo = true));
            }
        }

        public bool Foo
        {
            get
            {
                return this.foo;
            }

            set
            {
                if (this.foo != value)
                {
                    this.foo = value;
                    this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

                await this.VerifyHappyPathAsync(commandCode, testCode).ConfigureAwait(false);
            }

            [Test]
            public async Task IgnoringLazyNullCoalesceExpressionBody()
            {
                var commandCode = @"
namespace RoslynSandBox
{
    using System;
    using System.Windows.Input;

    public class DelegateCommand : ICommand
    {
        public DelegateCommand(Func<object, bool> func)
        {
            throw new NotImplementedException();
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}";
                var testCode = @"
namespace RoslynSandBox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ViewModel : INotifyPropertyChanged
    {
        private bool foo;
        private DelegateCommand fooCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public DelegateCommand FooCommand => this.fooCommand ?? (this.fooCommand = new DelegateCommand(param => this.Foo = true));

        public bool Foo
        {
            get
            {
                return this.foo;
            }

            set
            {
                if (this.foo != value)
                {
                    this.foo = value;
                    this.OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

                await this.VerifyHappyPathAsync(commandCode, testCode).ConfigureAwait(false);
            }

            [Test]
            public async Task IgnoreInCtor()
            {
                var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string name;

    public ViewModel(string name)
    {
        this.name = name;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name => this.name;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

                await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
            }

            [Test]
            public async Task IgnoreInInitializer()
            {
                var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string name;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name => this.name;

    public ViewModel Create(string name)
    {
        return new ViewModel { name = name };
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

                await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
            }

            [Test(Description = "We let WPF1010 nag about this.")]
            public async Task IgnoreSimplePropertyWithBackingField()
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
        get { return this.value; }
        set { this.value = value; }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

                await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
            }

            [Test]
            public async Task AssigningFieldsInGetter()
            {
                var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string name;
    private int getCount;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name
    {
        get
        {
            this.getCount++;
            return this.name;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

                await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
            }

            [Test]
            public async Task LazyGetter()
            {
                var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string name;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name
    {
        get
        {
            if (this.name == null)
            {
                this.name = string.Empty;
            }

            return this.name;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

                await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
            }

            [Test]
            public async Task LazyGetterExpressionBody()
            {
                var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string name;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name => this.name ?? (this.name = string.Empty);

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

                await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
            }

            [Test]
            public async Task IgnoreDisposeMethod()
            {
                var testCode = @"
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public sealed class ViewModel : INotifyPropertyChanged, IDisposable
{
    private string name;
    private bool disposed;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name
    {
        get
        {
            this.ThrowIfDisposed();
            return this.name ?? (this.name = string.Empty);
        }
    }

    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void ThrowIfDisposed()
    {
        if (this.disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}";

                await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
            }
        }
    }
}