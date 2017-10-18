namespace WpfAnalyzers.Test.WPF0030BackingFieldShouldBeStaticReadonlyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class CodeFix
    {
        [TestCase("public static", "public static readonly")]
        [TestCase("public", "public static readonly")]
        [TestCase("public readonly", "public static readonly")]
        [TestCase("private static", "private static readonly")]
        [TestCase("private", "private static readonly")]
        public void DependencyProperty(string before, string after)
        {
            var testCode = @"
namespace RoslynSandbox
{
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
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
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
    }
}";
            testCode = testCode.AssertReplace("public static DependencyProperty", before + " DependencyProperty");
            fixedCode = fixedCode.AssertReplace("public static readonly DependencyProperty", after + " DependencyProperty");
            AnalyzerAssert.CodeFix<WPF0030BackingFieldShouldBeStaticReadonly, MakeFieldStaticReadonlyCodeFixProvider>(testCode, fixedCode);
        }

        [TestCase("FooControl")]
        [TestCase("FooControl<T>")]
        public void DependencyPropertyAddOwner(string typeName)
        {
            var fooCode = @"
namespace RoslynSandbox
{
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
}";
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        ↓public static DependencyProperty BarProperty = Foo.BarProperty.AddOwner(typeof(FooControl));

        public double Bar
        {
            get { return (double)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
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
    }
}";
            testCode = testCode.AssertReplace("FooControl", typeName);
            fixedCode = fixedCode.AssertReplace("FooControl", typeName);
            AnalyzerAssert.CodeFix<WPF0030BackingFieldShouldBeStaticReadonly, MakeFieldStaticReadonlyCodeFixProvider>(new[] { fooCode, testCode }, fixedCode);
        }

        [Test]
        public void ReadOnlyDependencyProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
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
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
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
            AnalyzerAssert.CodeFix<WPF0030BackingFieldShouldBeStaticReadonly, MakeFieldStaticReadonlyCodeFixProvider>(testCode, fixedCode);
        }

        [Test]
        public void AttachedProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
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
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
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
            AnalyzerAssert.CodeFix<WPF0030BackingFieldShouldBeStaticReadonly, MakeFieldStaticReadonlyCodeFixProvider>(testCode, fixedCode);
        }

        [Test]
        public void ReadOnlyAttachedPropertyKey()
        {
            var testCode = @"
namespace RoslynSandbox
{
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
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
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
            AnalyzerAssert.CodeFix<WPF0030BackingFieldShouldBeStaticReadonly, MakeFieldStaticReadonlyCodeFixProvider>(testCode, fixedCode);
        }
    }
}
