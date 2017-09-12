namespace WpfAnalyzers.Test.PropertyChanged.WPF1010MutablePublicPropertyShouldNotifyTests
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.PropertyChanged;

    internal class CodeFix : CodeFixVerifier<WPF1010MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>
    {
        [Test]
        public async Task AutoPropertyExplicitName()
        {
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Bar { get; set; }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private int bar;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar
    {
        get
        {
            return this.bar;
        }

        set
        {
            if (value == this.bar)
            {
                return;
            }

            this.bar = value;
            this.OnPropertyChanged(nameof(this.Bar));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertyCallerMemberNameName()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Bar { get; set; }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int bar;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar
    {
        get
        {
            return this.bar;
        }

        set
        {
            if (value == this.bar)
            {
                return;
            }

            this.bar = value;
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

        [Test]
        public async Task AutoPropertyMvvmFramework()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MvvmFramework;

public class Foo : ViewModelBase
{
    ↓public int Bar { get; set; }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MvvmFramework;

public class Foo : ViewModelBase
{
    private int bar;

    public int Bar
    {
        get
        {
            return this.bar;
        }

        set
        {
            if (value == this.bar)
            {
                return;
            }

            this.bar = value;
            this.OnPropertyChanged();
        }
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertyCallerMemberNameNameUnderscoreNames()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Bar { get; set; }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int _bar;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar
    {
        get
        {
            return _bar;
        }

        set
        {
            if (value == _bar)
            {
                return;
            }

            _bar = value;
            OnPropertyChanged();
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
        public async Task AutoPropertyPropertyChangedEventArgs()
        {
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Bar { get; set; }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        this.PropertyChanged?.Invoke(this, e);
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            ////            var fixedCode = @"
            ////using System.ComponentModel;

            ////public class Foo : INotifyPropertyChanged
            ////{
            ////    private int bar;

            ////    public event PropertyChangedEventHandler PropertyChanged;

            ////    public int Bar
            ////    {
            ////        get
            ////        {
            ////            return this.bar;
            ////        }

            ////        set
            ////        {
            ////            if (value == this.bar)
            ////            {
            ////                return;
            ////            }

            ////            this.bar = value;
            ////            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Bar)));
            ////        }
            ////    }

            ////    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
            ////    {
            ////        this.PropertyChanged?.Invoke(this, e);
            ////    }
            ////}";

            // Not sure how we want this, asserting no fix for now
            await this.VerifyCSharpFixAsync(testCode, testCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertyPropertyChangedEventArgsAndCallerMemberName()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Bar { get; set; }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        this.PropertyChanged?.Invoke(this, e);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            ////            var fixedCode = @"
            ////using System.ComponentModel;
            ////using System.Runtime.CompilerServices;
            ////public class Foo : INotifyPropertyChanged
            ////{
            ////    private int bar;

            ////    public event PropertyChangedEventHandler PropertyChanged;

            ////    public int Bar
            ////    {
            ////        get
            ////        {
            ////            return this.bar;
            ////        }

            ////        set
            ////        {
            ////            if (value == this.bar)
            ////            {
            ////                return;
            ////            }

            ////            this.bar = value;
            ////            this.OnPropertyChanged();
            ////        }
            ////    }

            ////    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
            ////    {
            ////        this.PropertyChanged?.Invoke(this, e);
            ////    }

            ////    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            ////    {
            ////        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            ////    }
            ////}";

            // Not sure how we want this, asserting no fix for now
            await this.VerifyCSharpFixAsync(testCode, testCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertyPrivateSet()
        {
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
     public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Bar { get; private set; }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private int bar;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar
    {
        get
        {
            return this.bar;
        }

        private set
        {
            if (value == this.bar)
            {
                return;
            }

            this.bar = value;
            this.OnPropertyChanged(nameof(this.Bar));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertyInitialized()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ↓public bool IsTrue { get; private set; } = true;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("IsTrue");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private bool _isTrue = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsTrue
        {
            get
            {
                return _isTrue;
            }

            private set
            {
                if (value == _isTrue)
                {
                    return;
                }

                _isTrue = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertyGeneric()
        {
            var testCode = @"
using System.ComponentModel;

public class ViewModel<T> : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public T Bar { get; set; }

    protected virtual void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}";
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Property 'Bar' must notify when value changes.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class ViewModel<T> : INotifyPropertyChanged
{
    private T bar;

    public event PropertyChangedEventHandler PropertyChanged;

    public T Bar
    {
        get
        {
            return this.bar;
        }

        set
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(value, this.bar))
            {
                return;
            }

            this.bar = value;
            this.OnPropertyChanged(nameof(this.Bar));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertyConstrainedGenericReferenceType()
        {
            var testCode = @"
using System.ComponentModel;

public class ViewModel<T> : INotifyPropertyChanged
    where T : class
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public T Bar { get; set; }

    protected virtual void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}";
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Property 'Bar' must notify when value changes.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class ViewModel<T> : INotifyPropertyChanged
    where T : class
{
    private T bar;

    public event PropertyChangedEventHandler PropertyChanged;

    public T Bar
    {
        get
        {
            return this.bar;
        }

        set
        {
            if (ReferenceEquals(value, this.bar))
            {
                return;
            }

            this.bar = value;
            this.OnPropertyChanged(nameof(this.Bar));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertyConstrainedGenericValueType()
        {
            var testCode = @"
using System.ComponentModel;

public class ViewModel<T> : INotifyPropertyChanged
    where T : struct
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public T Bar { get; set; }

    protected virtual void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}";
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithMessage("Property 'Bar' must notify when value changes.");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class ViewModel<T> : INotifyPropertyChanged
    where T : struct
{
    private T bar;

    public event PropertyChangedEventHandler PropertyChanged;

    public T Bar
    {
        get
        {
            return this.bar;
        }

        set
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(value, this.bar))
            {
                return;
            }

            this.bar = value;
            this.OnPropertyChanged(nameof(this.Bar));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertyInsertCreatedFieldSorted()
        {
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private int a;
    private int c;

    public event PropertyChangedEventHandler PropertyChanged;

    public int A
    {
        get
        {
            return this.a;
        }

        set
        {
            if (value == this.a)
            {
                return;
            }

            this.a = value;
            this.OnPropertyChanged(nameof(this.A));
        }
    }

    ↓public int B { get; set; }

    public int C
    {
        get
        {
            return this.c;
        }

        set
        {
            if (value == this.c)
            {
                return;
            }

            this.c = value;
            this.OnPropertyChanged(nameof(this.C));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("B");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private int a;
    private int b;
    private int c;

    public event PropertyChangedEventHandler PropertyChanged;

    public int A
    {
        get
        {
            return this.a;
        }

        set
        {
            if (value == this.a)
            {
                return;
            }

            this.a = value;
            this.OnPropertyChanged(nameof(this.A));
        }
    }

    public int B
    {
        get
        {
            return this.b;
        }

        set
        {
            if (value == this.b)
            {
                return;
            }

            this.b = value;
            this.OnPropertyChanged(nameof(this.B));
        }
    }

    public int C
    {
        get
        {
            return this.c;
        }

        set
        {
            if (value == this.c)
            {
                return;
            }

            this.c = value;
            this.OnPropertyChanged(nameof(this.C));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task AutoPropertyWhenFieldExists()
        {
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private int bar;

    public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Bar { get; set; }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private int bar;
    private int bar_;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Bar
    {
        get
        {
            return this.bar_;
        }

        set
        {
            if (value == this.bar_)
            {
                return;
            }

            this.bar_ = value;
            this.OnPropertyChanged(nameof(this.Bar));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task WithBackingFieldExplicitName()
        {
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Value
    {
        get
        {
            return this.value;
        }
        private set
        {
            this.value = value;
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Value");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Value
    {
        get
        {
            return this.value;
        }

        private set
        {
            if (value == this.value)
            {
                return;
            }

            this.value = value;
            this.OnPropertyChanged(nameof(this.Value));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task WithBackingFieldCallerMemberName()
        {
            var testCode = @"using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Value
    {
        get
        {
            return this.value;
        }
        private set
        {
            this.value = value;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Value");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Value
    {
        get
        {
            return this.value;
        }

        private set
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

        [Test]
        public async Task WithNestedBackingField()
        {
            var barCode = @"namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Bar : INotifyPropertyChanged
    {
        private int _value;

        public event PropertyChangedEventHandler PropertyChanged;

        ↓public int Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (value == _value)
                {
                    return;
                }

                _value = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var testCode = @"namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly Bar bar = new Bar();

        public event PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get { return this.bar.Value; }
            set { this.bar.Value = value; }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Value");
            await this.VerifyCSharpDiagnosticAsync(new[] { barCode, testCode }, expected).ConfigureAwait(false);

            var fixedCode = @"namespace RoslynSandbox
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Foo : INotifyPropertyChanged
    {
        private readonly Bar bar = new Bar();

        public event PropertyChangedEventHandler PropertyChanged;

    public int Value
    {
        get
        {
            return this.bar.value;
        }

        private set
        {
            if (value == this.bar.Value)
            {
                return;
            }

            this.bar.value = value;
            this.OnPropertyChanged();
        }
    }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";
            await this.VerifyCSharpFixAsync(new[] { barCode, testCode }, new[] { barCode, fixedCode }, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task WithBackingFieldCallerMemberNameAccessorsOnOneLine()
        {
            var testCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Value
    {
        get { return this.value; }
        private set { this.value = value; }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Value");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Foo : INotifyPropertyChanged
{
    private int value;

    public event PropertyChangedEventHandler PropertyChanged;

    public int Value
    {
        get
        {
            return this.value;
        }

        private set
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

        [Test]
        public async Task AutoPropertyExplicitNameHandlesRecursion()
        {
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public int Bar { get; set; }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.OnPropertyChanged(propertyName);
    }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        this.PropertyChanged?.Invoke(this, e);
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            await this.VerifyCSharpFixAsync(testCode, testCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        public async Task IgnoresWhenBaseHasPropertyChangedEventButNoInterface()
        {
            Assert.Inconclusive("Not sure if there is a clean way. Not common enough for special casing. Maybe ask for a fix on uservoice :D");
            var testCode = @"
namespace RoslynSandBox
{
    using System.Windows.Input;

    public class CustomGesture : MouseGesture
    {
        ↓public int Foo { get; set; }
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Foo");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            await this.VerifyCSharpFixAsync(testCode, testCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }
    }
}