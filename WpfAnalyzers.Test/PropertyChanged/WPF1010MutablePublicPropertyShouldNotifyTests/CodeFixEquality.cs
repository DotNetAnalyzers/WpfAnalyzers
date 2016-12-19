namespace WpfAnalyzers.Test.PropertyChanged.WPF1010MutablePublicPropertyShouldNotifyTests
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.PropertyChanged;

    internal class CodeFixEquality : CodeFixVerifier<WPF1010MutablePublicPropertyShouldNotify, MakePropertyNotifyCodeFixProvider>
    {
        [TestCase("int")]
        [TestCase("int?")]
        [TestCase("Nullable<int>")]
        [TestCase("string")]
        [TestCase("StringComparison")]
        public async Task OpEqualsFor(string typeCode)
        {
            var testCode = @"
using System;
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
            testCode = testCode.AssertReplace("int", typeCode);
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System;
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
            fixedCode = fixedCode.AssertReplace("int", typeCode);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, allowNewCompilerDiagnostics: true)
                    .ConfigureAwait(false);
        }

        [Test]
        public async Task ReferenceType()
        {
            var refTypeCode = @"
public class ReferenceType
{
}";
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public ReferenceType Bar { get; set; }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(new[] { refTypeCode, testCode }, expected)
                      .ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private ReferenceType bar;

    public event PropertyChangedEventHandler PropertyChanged;

    public ReferenceType Bar
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

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";
            await this.VerifyCSharpFixAsync(new[] { refTypeCode, testCode }, new[] { refTypeCode, fixedCode }, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task EquatableStruct()
        {
            var equatableStruct = @"
using System;
public struct EquatableStruct : IEquatable<EquatableStruct>
{
    public readonly int Value;

    public EquatableStruct(int value)
    {
        this.Value = value;
    }

    public bool Equals(EquatableStruct other)
    {
        return this.Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is EquatableStruct && Equals((EquatableStruct)obj);
    }

    public override int GetHashCode()
    {
        return this.Value;
    }
}
";
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public EquatableStruct Bar { get; set; }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(new[] { equatableStruct, testCode }, expected)
                      .ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private EquatableStruct bar;

    public event PropertyChangedEventHandler PropertyChanged;

    public EquatableStruct Bar
    {
        get
        {
            return this.bar;
        }

        set
        {
            if (value.Equals(this.bar))
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
            await this.VerifyCSharpFixAsync(new[] { equatableStruct, testCode }, new[] { equatableStruct, fixedCode }, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task NullableEquatableStruct()
        {
            var equatableStruct = @"
using System;
public struct EquatableStruct : IEquatable<EquatableStruct>
{
    public readonly int Value;

    public EquatableStruct(int value)
    {
        this.Value = value;
    }

    public bool Equals(EquatableStruct other)
    {
        return this.Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is EquatableStruct && Equals((EquatableStruct)obj);
    }

    public override int GetHashCode()
    {
        return this.Value;
    }
}
";
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public EquatableStruct? Bar { get; set; }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(new[] { equatableStruct, testCode }, expected)
                      .ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private EquatableStruct? bar;

    public event PropertyChangedEventHandler PropertyChanged;

    public EquatableStruct? Bar
    {
        get
        {
            return this.bar;
        }

        set
        {
            if (System.Nullable.Equals(value, this.bar))
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
            await this.VerifyCSharpFixAsync(new[] { equatableStruct, testCode }, new[] { equatableStruct, fixedCode }, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task EquatableStructWithOpEquals()
        {
            var equatableStruct = @"
using System;

public struct EquatableStruct : IEquatable<EquatableStruct>
{
    public readonly int Value;


    public EquatableStruct(int value)
    {
        this.Value = value;
    }

    public static bool operator ==(EquatableStruct left, EquatableStruct right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(EquatableStruct left, EquatableStruct right)
    {
        return !left.Equals(right);
    }

    public bool Equals(EquatableStruct other)
    {
        return this.Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is EquatableStruct && Equals((EquatableStruct)obj);
    }

    public override int GetHashCode()
    {
        return this.Value;
    }
}
";
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public EquatableStruct Bar { get; set; }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(new[] { equatableStruct, testCode }, expected)
                      .ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private EquatableStruct bar;

    public event PropertyChangedEventHandler PropertyChanged;

    public EquatableStruct Bar
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
            await this.VerifyCSharpFixAsync(new[] { equatableStruct, testCode }, new[] { equatableStruct, fixedCode }, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task NullableEquatableStructOpEquals()
        {
            var equatableStruct = @"
using System;

public struct EquatableStruct : IEquatable<EquatableStruct>
{
    public readonly int Value;

    public EquatableStruct(int value)
    {
        this.Value = value;
    }

    public static bool operator ==(EquatableStruct left, EquatableStruct right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(EquatableStruct left, EquatableStruct right)
    {
        return !left.Equals(right);
    }

    public bool Equals(EquatableStruct other)
    {
        return this.Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is EquatableStruct && Equals((EquatableStruct)obj);
    }

    public override int GetHashCode()
    {
        return this.Value;
    }
}
";
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public EquatableStruct? Bar { get; set; }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(new[] { equatableStruct, testCode }, expected)
                      .ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private EquatableStruct? bar;

    public event PropertyChangedEventHandler PropertyChanged;

    public EquatableStruct? Bar
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
            await this.VerifyCSharpFixAsync(new[] { equatableStruct, testCode }, new[] { equatableStruct, fixedCode }, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task NotEquatableStruct()
        {
            var equatableStruct = @"
public struct NotEquatableStruct
{
    public readonly int Value;

    public NotEquatableStruct(int value)
    {
        this.Value = value;
    }
}
";
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public NotEquatableStruct Bar { get; set; }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(new[] { equatableStruct, testCode }, expected)
                      .ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private NotEquatableStruct bar;

    public event PropertyChangedEventHandler PropertyChanged;

    public NotEquatableStruct Bar
    {
        get
        {
            return this.bar;
        }

        set
        {
            if (System.Collections.Generic.EqualityComparer<NotEquatableStruct>.Default.Equals(value, this.bar))
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
            await this.VerifyCSharpFixAsync(new[] { equatableStruct, testCode }, new[] { equatableStruct, fixedCode }, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }

        [Test]
        public async Task NullableNotEquatableStruct()
        {
            var equatableStruct = @"
public struct NotEquatableStruct
{
    public readonly int Value;

    public NotEquatableStruct(int value)
    {
        this.Value = value;
    }
}
";
            var testCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    ↓public NotEquatableStruct? Bar { get; set; }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}";

            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(ref testCode)
                               .WithArguments("Bar");
            await this.VerifyCSharpDiagnosticAsync(new[] { equatableStruct, testCode }, expected)
                      .ConfigureAwait(false);

            var fixedCode = @"
using System.ComponentModel;

public class Foo : INotifyPropertyChanged
{
    private NotEquatableStruct? bar;

    public event PropertyChangedEventHandler PropertyChanged;

    public NotEquatableStruct? Bar
    {
        get
        {
            return this.bar;
        }

        set
        {
            if (System.Nullable.Equals(value, this.bar))
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
            await this.VerifyCSharpFixAsync(new[] { equatableStruct, testCode }, new[] { equatableStruct, fixedCode }, allowNewCompilerDiagnostics: true)
                      .ConfigureAwait(false);
        }
    }
}