namespace WpfAnalyzers.Test.WPF0015RegisteredOwnerTypeMustBeDependencyObjectTests
{
    using System.Threading.Tasks;
    using NUnit.Framework;
    using WPF0015RegisteredOwnerTypeMustBeDependencyObject = WpfAnalyzers.WPF0015RegisteredOwnerTypeMustBeDependencyObject;

    internal class Diagnostics : DiagnosticVerifier<WPF0015RegisteredOwnerTypeMustBeDependencyObject>
    {
        [Test]
        public async Task DependencyRegister()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(int),
            ↓typeof(string),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("RegisterAttached");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertyRegisterReadOnly()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
            ""Bar"",
            typeof(int),
            ↓typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("RegisterAttached");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertyAddOwner()
        {
            var part1 = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl
    {
        public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(↓typeof(FooControl));
    }
}";

            var part2 = @"
namespace RoslynSandbox
{
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
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref part1).WithArguments("RegisterAttached");
            await this.VerifyCSharpDiagnosticAsync(new[] { part1, part2 }, expected).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertyOverrideMetadata()
        {
            var fooControlCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            ""Value"",
            typeof(int),
            typeof(Control),
            new PropertyMetadata(default(int)));
    }
}";

            var barControlCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class BarControl : FooControl
    {
        static BarControl()
        {
            ValueProperty.OverrideMetadata(↓typeof(BarControl), new PropertyMetadata(1));
        }
    }
}";

            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref barControlCode).WithArguments("RegisterAttached");
            await this.VerifyCSharpDiagnosticAsync(new[] { fooControlCode, barControlCode }, expected).ConfigureAwait(false);
        }
    }
}