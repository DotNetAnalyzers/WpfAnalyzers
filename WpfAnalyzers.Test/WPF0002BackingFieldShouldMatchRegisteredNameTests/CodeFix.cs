namespace WpfAnalyzers.Test.WPF0002BackingFieldShouldMatchRegisteredNameTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class CodeFix
{
    private static readonly DependencyPropertyBackingFieldOrPropertyAnalyzer Analyzer = new();
    private static readonly RenameMemberFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0002BackingFieldShouldMatchRegisteredName);

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
        private static readonly DependencyPropertyKey ↓Error = DependencyProperty.RegisterReadOnly(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = Error.DependencyProperty;

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(Error, value); }
        }
    }
}";

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Field 'Error' that is backing field for the DependencyPropertyKey registered as 'Bar' must be named 'BarPropertyKey'"), code);
    }

    [Test]
    public static void DependencyPropertyRegisterReadOnlyBackingFields()
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static readonly DependencyPropertyKey ↓Error = DependencyProperty.RegisterReadOnly(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = Error.DependencyProperty;

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(Error, value); }
        }
    }
}";

        var after = @"
namespace N
{
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
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DependencyPropertyRegisterReadOnlyBackingProperties()
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static DependencyPropertyKey ↓Error { get; } = DependencyProperty.RegisterReadOnly(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static DependencyProperty BarProperty { get; } = Error.DependencyProperty;

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(Error, value); }
        }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static DependencyPropertyKey BarPropertyKey { get; } = DependencyProperty.RegisterReadOnly(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static DependencyProperty BarProperty { get; } = BarPropertyKey.DependencyProperty;

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarPropertyKey, value); }
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DependencyPropertyRegisterAttachedReadOnlyBackingFields()
    {
        var before = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        private static readonly DependencyPropertyKey ↓Error = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = Error.DependencyProperty;

        public static void SetBar(DependencyObject element, int value)
        {
            element.SetValue(Error, value);
        }

        public static int GetBar(DependencyObject element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

        var after = @"
namespace N
{
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
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DependencyPropertyRegisterAttachedReadOnlyBackingProperties()
    {
        var before = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        private static DependencyPropertyKey ↓Error { get; } = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static DependencyProperty BarProperty { get; } = Error.DependencyProperty;

        public static void SetBar(DependencyObject element, int value)
        {
            element.SetValue(Error, value);
        }

        public static int GetBar(DependencyObject element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        private static DependencyPropertyKey BarPropertyKey { get; } = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static DependencyProperty BarProperty { get; } = BarPropertyKey.DependencyProperty;

        public static void SetBar(DependencyObject element, int value)
        {
            element.SetValue(BarPropertyKey, value);
        }

        public static int GetBar(DependencyObject element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }
}
