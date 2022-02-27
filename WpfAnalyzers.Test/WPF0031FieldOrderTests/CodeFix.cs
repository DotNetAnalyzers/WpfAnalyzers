namespace WpfAnalyzers.Test.WPF0031FieldOrderTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class CodeFix
{
    private static readonly DependencyPropertyBackingFieldOrPropertyAnalyzer Analyzer = new();
    private static readonly MoveFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0031FieldOrder);

    [Test]
    public static void Messages()
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ↓BarPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get => (int)this.GetValue(BarProperty);
            protected set => this.SetValue(BarPropertyKey, value);
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
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public int Bar
        {
            get => (int)this.GetValue(BarProperty);
            protected set => this.SetValue(BarPropertyKey, value);
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic.WithMessage("'BarPropertyKey' must be declared before 'BarProperty'"), before, after, fixTitle: "Move");
    }

    [Test]
    public static void ReadOnlyDependencyProperty()
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ↓BarPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get => (int)this.GetValue(BarProperty);
            protected set => this.SetValue(BarPropertyKey, value);
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
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public int Bar
        {
            get => (int)this.GetValue(BarProperty);
            protected set => this.SetValue(BarPropertyKey, value);
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void ReadOnlyDependencyPropertyWithComments()
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        // DependencyProperty
        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        // DependencyPropertyKey
        private static readonly DependencyPropertyKey ↓BarPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get => (int)this.GetValue(BarProperty);
            protected set => this.SetValue(BarPropertyKey, value);
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
        // DependencyPropertyKey
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        // DependencyProperty
        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public int Bar
        {
            get => (int)this.GetValue(BarProperty);
            protected set => this.SetValue(BarPropertyKey, value);
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void ReadOnlyAttachedProperty()
    {
        var before = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ↓BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

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
}