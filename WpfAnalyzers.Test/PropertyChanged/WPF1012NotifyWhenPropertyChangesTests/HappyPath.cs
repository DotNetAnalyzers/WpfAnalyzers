namespace WpfAnalyzers.Test.PropertyChanged.WPF1012NotifyWhenPropertyChangesTests
{
    using System.Threading.Tasks;
    using NUnit.Framework;
    using WpfAnalyzers.PropertyChanged;

    internal class HappyPath : HappyPathVerifier<WPF1012NotifyWhenPropertyChanges>
    {
        [TestCase("null")]
        [TestCase("string.Empty")]
        [TestCase(@"""Bar""")]
        [TestCase(@"nameof(Bar)")]
        [TestCase(@"nameof(this.Bar)")]
        public async Task NoCalculated(string propertyName)
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
                this.OnPropertyChanged(nameof(Bar));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }";

            testCode = testCode.AssertReplace(@"nameof(Bar)", propertyName);
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotifying()
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

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotifyingMvvmFramework()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MvvmFramework;

public class ViewModel : ViewModelBase
{
    private string firstName;
    private string lastName;

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
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task CallsOnPropertyChangedWithCachedEventArgs()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private static readonly PropertyChangedEventArgs FirstNameArgs = new PropertyChangedEventArgs(nameof(FirstName));
    private static readonly PropertyChangedEventArgs LastNameArgs = new PropertyChangedEventArgs(nameof(LastName));
    private static readonly PropertyChangedEventArgs FullNameArgs = new PropertyChangedEventArgs(nameof(FullName));

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
            this.OnPropertyChanged(FirstNameArgs);
            this.OnPropertyChanged(FullNameArgs);
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
            this.OnPropertyChanged(LastNameArgs);
            this.OnPropertyChanged(FullNameArgs);
        }
    }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        this.PropertyChanged?.Invoke(this, e);
    }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotifyingSettingFieldInMethod()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private string name;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name => this.name;

    protected virtual void UpdateName(string name)
    {
        this.name = name;
        this.OnPropertyChanged(nameof(Name));
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task WhenNotifyingSettingFieldInMethodOutsideLock()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModel : INotifyPropertyChanged
{
    private readonly object gate = new object();
    private string name;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Name => this.name;

    protected virtual void UpdateName(string name)
    {
        lock (this.gate)
        {
            this.name = name;
        }

        this.OnPropertyChanged(nameof(Name));
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
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
        public async Task NotifyingInLambda()
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