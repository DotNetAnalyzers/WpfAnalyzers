namespace WpfAnalyzers.Test.DependencyProperties.WPF0030BackingFieldShouldBeStaticReadonly
{
    using System.Threading;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class CodeFix : CodeFixVerifier<WPF0030BackingFieldShouldBeStaticReadonly, MakeFieldStaticReadonlyCodeFixProvider>
    {
        [TestCase("public static", "public static readonly")]
        [TestCase("public", "public static readonly")]
        [TestCase("public readonly", "public static readonly")]
        [TestCase("private static", "private static readonly")]
        [TestCase("private", "private static readonly")]
        public async Task DependencyProperty(string before, string after)
        {
            var testCode = @"
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        ↓public static DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"", typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }";
            testCode = testCode.AssertReplace("public static DependencyProperty", before + " DependencyProperty");
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("BarProperty", "DependencyProperty", "Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"", typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }";
            fixedCode = fixedCode.AssertReplace("public static readonly DependencyProperty", after + " DependencyProperty");
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [TestCase("FooControl")]
        [TestCase("FooControl<T>")]
        public async Task DependencyPropertyAddOwner(string typeName)
        {
            var testCode = @"
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
}

public class FooControl : Control
{
    ↓public static DependencyProperty BarProperty = Foo.BarProperty.AddOwner(typeof(FooControl));

    public double Bar
    {
        get { return (double)this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }
}";
            testCode = testCode.AssertReplace("FooControl", typeName);
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("BarProperty");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
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
}

public class FooControl : Control
{
    public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(typeof(FooControl));

    public double Bar
    {
        get { return (double)this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }
}";
            fixedCode = fixedCode.AssertReplace("FooControl", typeName);
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task ReadOnlyDependencyProperty()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    ↓private static DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
        ""Bar"",
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public int Bar
    {
        get { return (int)GetValue(BarProperty); }
        set { SetValue(BarPropertyKey, value); }
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("BarPropertyKey");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
        ""Bar"",
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public int Bar
    {
        get { return (int)GetValue(BarProperty); }
        set { SetValue(BarPropertyKey, value); }
    }
}";
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task AttachedProperty()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

    ↓public static DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public static void SetBar(DependencyObject element, int value)
    {
        element.SetValue(BarPropertyKey, value);
    }

    public static int GetBar(DependencyObject element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("BarProperty", "DependencyProperty", "Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

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
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task ReadOnlyAttachedPropertyKey()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    ↓private static DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
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

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("BarPropertyKey");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

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
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }
    }
}
