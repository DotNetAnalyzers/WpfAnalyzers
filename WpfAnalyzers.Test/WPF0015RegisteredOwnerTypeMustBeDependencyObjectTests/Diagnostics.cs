namespace WpfAnalyzers.Test.WPF0015RegisteredOwnerTypeMustBeDependencyObjectTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class Diagnostics
    {
        [Test]
        public void Message()
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
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                "WPF0015",
                "Maybe you intended to use 'RegisterAttached'?");
            AnalyzerAssert.Diagnostics<WPF0015RegisteredOwnerTypeMustBeDependencyObject>(expectedDiagnostic, testCode);
        }

        [Test]
        public void DependencyRegister()
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
            AnalyzerAssert.Diagnostics<WPF0015RegisteredOwnerTypeMustBeDependencyObject>(testCode);
        }

        [Test]
        public void DependencyPropertyRegisterReadOnly()
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

            AnalyzerAssert.Diagnostics<WPF0015RegisteredOwnerTypeMustBeDependencyObject>(testCode);
        }

        [Test]
        public void DependencyPropertyAddOwner()
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

            AnalyzerAssert.Diagnostics<WPF0015RegisteredOwnerTypeMustBeDependencyObject>(part1, part2);
        }

        [Test]
        public void DependencyPropertyOverrideMetadata()
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

            AnalyzerAssert.Diagnostics<WPF0015RegisteredOwnerTypeMustBeDependencyObject>(fooControlCode, barControlCode);
        }
    }
}