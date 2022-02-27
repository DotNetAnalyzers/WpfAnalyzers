namespace WpfAnalyzers.Test.WPF0015RegisteredOwnerTypeMustBeDependencyObjectTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Diagnostics
{
    private static readonly WPF0015RegisteredOwnerTypeMustBeDependencyObject Analyzer = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0015RegisteredOwnerTypeMustBeDependencyObject);

    [Test]
    public static void Message()
    {
        var code = @"
namespace N
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
        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Maybe you intended to use 'RegisterAttached'?"), code);
    }

    [Test]
    public static void DependencyRegister()
    {
        var code = @"
namespace N
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
        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }

    [Test]
    public static void DependencyPropertyRegisterReadOnly()
    {
        var code = @"
namespace N
{
    using System.Windows;

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

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }

    [Test]
    public static void DependencyPropertyAddOwner()
    {
        var code = @"
namespace N
{
    using System.Windows;

    public class FooControl
    {
        public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(↓typeof(FooControl));
    }
}";

        var part2 = @"
namespace N
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

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code, part2);
    }

    [Test]
    public static void DependencyPropertyOverrideMetadata()
    {
        var fooControlCode = @"
namespace N
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
namespace N
{
    using System.Windows;

    public class BarControl : FooControl
    {
        static BarControl()
        {
            ValueProperty.OverrideMetadata(↓typeof(BarControl), new PropertyMetadata(1));
        }
    }
}";

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, fooControlCode, barControlCode);
    }
}
