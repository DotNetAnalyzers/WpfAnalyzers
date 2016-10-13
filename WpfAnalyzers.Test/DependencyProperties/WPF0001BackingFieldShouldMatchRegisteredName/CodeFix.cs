namespace WpfAnalyzers.Test.DependencyProperties.WPF0001BackingFieldShouldMatchRegisteredName
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class CodeFix : CodeFixVerifier<WPF0001BackingFieldShouldMatchRegisteredName, RenameFieldCodeFixProvider>
    {
        [Test]
        public async Task DependencyProperty()
        {
            var testCode = @"
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ↓Error = DependencyProperty.Register(
            ""Bar"", typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(Error); }
            set { SetValue(Error, value); }
        }
    }";
            var expected = this.CSharpDiagnostic()
                               .WithLocationIndicated(testCode, out testCode)
                               .WithArguments("Error", "Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

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
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertyPartial()
        {

            var part1 = @"
using System.Windows;
using System.Windows.Controls;

public partial class FooControl
{
    public static readonly DependencyProperty ErrorProperty = BarPropertyKey.DependencyProperty;

    public int Bar
    {
        get { return (int)GetValue(ErrorProperty); }
        set { SetValue(BarPropertyKey, value); }
    }
}";

            var part2 = @"
using System.Windows;
using System.Windows.Controls;

public partial class FooControl : Control
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
        ""Bar"",
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));
}";


            var expected = this.CSharpDiagnostic().WithLocation(7, 47).WithArguments("ErrorProperty", "Bar");
            await this.VerifyCSharpDiagnosticAsync(new[] { part1, part2 }, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public partial class FooControl
{
    public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public int Bar
    {
        get { return (int)GetValue(BarProperty); }
        set { SetValue(BarPropertyKey, value); }
    }
}";
            await this.VerifyCSharpFixAsync(new[] { part1, part2 }, new[] { fixedCode, part2 }).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertyAddOwner()
        {
            var part1 = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty Error = Foo.BarProperty.AddOwner(typeof(FooControl));

    public int Bar
    {
        get { return (int) this.GetValue(Error); }
        set { this.SetValue(Error, value); }
    }
}";

            var part2 = @"
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(int), 
        typeof(Foo), 
        new FrameworkPropertyMetadata(
            default(int), 
            FrameworkPropertyMetadataOptions.Inherits));

    public static void SetBar(DependencyObject element, int value)
    {
        element.SetValue(BarProperty, value);
    }

    public static int GetBar(DependencyObject element)
    {
        return (int) element.GetValue(BarProperty);
    }
}";

            var expected = this.CSharpDiagnostic().WithLocation(7, 47).WithArguments("Error", "Bar");
            await this.VerifyCSharpDiagnosticAsync(new[] { part1, part2 }, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(typeof(FooControl));

    public int Bar
    {
        get { return (int) this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }
}";
            await this.VerifyCSharpFixAsync(new[] { part1, part2 }, new[] { fixedCode, part2 }).ConfigureAwait(false);
        }

        [Test]
        public async Task ReadonlyDependencyProperty()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
        ""Bar"",
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty ErrorProperty = BarPropertyKey.DependencyProperty;

    public int Bar
    {
        get { return (int)GetValue(ErrorProperty); }
        set { SetValue(BarPropertyKey, value); }
    }
}";

            var expected = this.CSharpDiagnostic().WithLocation(13, 47).WithArguments("ErrorProperty", "Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

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
    public static readonly DependencyProperty Error = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

    public static void SetBar(DependencyObject element, int value)
    {
        element.SetValue(Error, value);
    }

    public static int GetBar(DependencyObject element)
    {
        return (int)element.GetValue(Error);
    }
}";

            var expected = this.CSharpDiagnostic().WithLocation(6, 47).WithArguments("Error", "Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

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
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [Test]
        public async Task ReadOnlyAttached()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    public static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty Error = BarPropertyKey.DependencyProperty;

    public static void SetBar(DependencyObject element, int value)
    {
        element.SetValue(BarPropertyKey, value);
    }

    public static int GetBar(DependencyObject element)
    {
        return (int)element.GetValue(Error);
    }
}";

            var expected = this.CSharpDiagnostic().WithLocation(12, 47).WithArguments("Error", "Bar");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;

public static class Foo
{
    public static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
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
