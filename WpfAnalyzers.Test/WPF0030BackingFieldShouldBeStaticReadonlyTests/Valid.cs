namespace WpfAnalyzers.Test.WPF0030BackingFieldShouldBeStaticReadonlyTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly DependencyPropertyBackingFieldOrPropertyAnalyzer Analyzer = new();

    [Test]
    public static void DependencyProperty()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"", typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void DependencyPropertyFullyQualified()
    {
        var code = @"
namespace N
{
    public class FooControl : System.Windows.Controls.Control
    {
        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static readonly System.Windows.DependencyProperty BarProperty = System.Windows.DependencyProperty.Register(
            ""Bar"", typeof(int), typeof(FooControl), new System.Windows.PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";

        RoslynAssert.Valid(Analyzer, code);
    }

    [TestCase("FooControl")]
    [TestCase("FooControl<T>")]
    public static void DependencyPropertyAddOwner(string typeName)
    {
        var fooCode = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
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
}";

        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(typeof(FooControl));

        public double Bar
        {
            get { return (double)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }
    }
}".AssertReplace("FooControl", typeName);

        RoslynAssert.Valid(Analyzer, fooCode, code);
    }

    [Test]
    public static void ReadOnlyDependencyProperty()
    {
        var code = @"
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

        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarPropertyKey, value); }
        }
    }
}";

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void DependencyPropertyRegisterAttached()
    {
        var code = @"
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
        RoslynAssert.Valid(Analyzer, code);
    }
}
