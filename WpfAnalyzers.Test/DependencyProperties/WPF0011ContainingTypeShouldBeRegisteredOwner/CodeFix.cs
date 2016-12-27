namespace WpfAnalyzers.Test.DependencyProperties.WPF0011ContainingTypeShouldBeRegisteredOwner
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class CodeFix : CodeFixVerifier<WPF0011ContainingTypeShouldBeRegisteredOwner, UseContainingTypeAsOwnerCodeFixProvider>
    {
        [TestCase("BarControl")]
        [TestCase("BarControl<T>")]
        public async Task DependencyPropertyRegister(string typeName)
        {
            var barControlCode = @"
using System.Windows;
using System.Windows.Controls;

public class BarControl : Control
{
}";

            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    // registering for an owner that is not containing type.
    public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
        nameof(Bar),
        typeof(int),
        ↓typeof(BarControl),
        new PropertyMetadata(default(int)));

    public int Bar
    {
        get { return (int)this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }
}";
            barControlCode = barControlCode.AssertReplace("class BarControl", $"class {typeName}");
            testCode = testCode.AssertReplace("typeof(BarControl)", $"typeof({typeName.Replace("<T>", "<int>")})");
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("FooControl");
            await this.VerifyCSharpDiagnosticAsync(new[] { testCode, barControlCode }, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    // registering for an owner that is not containing type.
    public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
        nameof(Bar),
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public int Bar
    {
        get { return (int)this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }
}";
            await this.VerifyCSharpFixAsync(new[] { testCode, barControlCode }, new[] { fixedCode, barControlCode }).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertyRegisterReadOnly()
        {
            var barControlCode = @"
using System.Windows;
using System.Windows.Controls;

public class BarControl : Control
{
}";

            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    // registering for an owner that is not containing type.
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
        ""Bar"",
        typeof(int),
        ↓typeof(BarControl),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public int Bar
    {
        get { return (int)this.GetValue(BarProperty); }
        protected set {  this.SetValue(BarPropertyKey, value);}
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("FooControl");
            await this.VerifyCSharpDiagnosticAsync(new[] { testCode, barControlCode }, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    // registering for an owner that is not containing type.
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
        ""Bar"",
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public int Bar
    {
        get { return (int)this.GetValue(BarProperty); }
        protected set {  this.SetValue(BarPropertyKey, value);}
    }
}";

            await this.VerifyCSharpFixAsync(new[] { testCode, barControlCode }, new[] { fixedCode, barControlCode }).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertyRegisterAttached()
        {
            var barCode = @"
public class Bar
{
}";
            var testCode = @"
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(int),
        ↓typeof(Bar),
        new PropertyMetadata(default(int)));

    public static void SetBar(DependencyObject element, int value)
    {
        element.SetValue(BarProperty, value);
    }

    public static int GetBar(DependencyObject element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Foo");
            await this.VerifyCSharpDiagnosticAsync(new[] { testCode, barCode }, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

    public static void SetBar(DependencyObject element, int value)
    {
        element.SetValue(BarProperty, value);
    }

    public static int GetBar(DependencyObject element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";

            await this.VerifyCSharpFixAsync(new[] { testCode, barCode }, new[] { fixedCode, barCode }).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertyRegisterAttachedReadOnly()
        {
            var barCode = @"
public class Bar
{
}";

            var testCode = @"
using System.Windows;

public static class Foo
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        ""Bar"",
        typeof(int),
        ↓typeof(Bar),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public static void SetBar(DependencyObject element, int value)
    {
        element.SetValue(BarPropertyKey, value);
    }

    public static int GetBar(DependencyObject element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("Foo");
            await this.VerifyCSharpDiagnosticAsync(new[] { testCode, barCode }, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;

public static class Foo
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public static void SetBar(DependencyObject element, int value)
    {
        element.SetValue(BarPropertyKey, value);
    }

    public static int GetBar(DependencyObject element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";
            await this.VerifyCSharpFixAsync(new[] { testCode, barCode }, new[] { fixedCode, barCode }).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertyAddOwner()
        {
            var fooCode = @"
using System.Windows;
using System.Windows.Controls;

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

    public static void SetBar(DependencyObject element, int value)
    {
        element.SetValue(BarProperty, value);
    }

    [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
    [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
    public static int GetBar(DependencyObject element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";

            var barControlCode = @"
using System.Windows;
using System.Windows.Controls;

public class BarControl : Control
{
}";
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(↓typeof(BarControl));

    public double Bar
    {
        get { return (double)this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("FooControl");
            await this.VerifyCSharpDiagnosticAsync(new[] { testCode, fooCode, barControlCode }, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(typeof(FooControl));

    public double Bar
    {
        get { return (double)this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }
}";

            await this.VerifyCSharpFixAsync(new[] { testCode, fooCode, barControlCode }, new[] { fixedCode, fooCode, barControlCode }).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertyOverrideMetadata()
        {
            var fooControlCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public int Value
    {
        get { return (int)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }
}";

            var barControlCode = @"
using System.Windows;
using System.Windows.Controls;

public class BarControl : FooControl
{
    static BarControl()
    {
        ValueProperty.OverrideMetadata(↓typeof(string), new PropertyMetadata(1));
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref barControlCode).WithArguments("BarControl");
            await this.VerifyCSharpDiagnosticAsync(new[] { fooControlCode, barControlCode }, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class BarControl : FooControl
{
    static BarControl()
    {
        ValueProperty.OverrideMetadata(typeof(BarControl), new PropertyMetadata(1));
    }
}";

            await this.VerifyCSharpFixAsync(new[] { fooControlCode, barControlCode }, new[] { fooControlCode, fixedCode }).ConfigureAwait(false);
        }
    }
}