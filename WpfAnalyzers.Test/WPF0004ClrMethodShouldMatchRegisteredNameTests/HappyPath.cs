namespace WpfAnalyzers.Test.WPF0004ClrMethodShouldMatchRegisteredNameTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class HappyPath
    {
        [TestCase("public")]
        [TestCase("private")]
        public void AttachedProperty(string accessModifier)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetBar(FrameworkElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int GetBar(FrameworkElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
            testCode = testCode.AssertReplace("    public", $"    {accessModifier}");
            AnalyzerAssert.Valid<WPF0004ClrMethodShouldMatchRegisteredName>(testCode);
        }

        [TestCase("public")]
        [TestCase("private")]
        public void AttachedPropertyExtensionMethods(string accessModifier)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetBar(this FrameworkElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int GetBar(this FrameworkElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
            testCode = testCode.AssertReplace("    public", $"    {accessModifier}");
            AnalyzerAssert.Valid<WPF0004ClrMethodShouldMatchRegisteredName>(testCode);
        }

        [TestCase("public")]
        [TestCase("private")]
        public void AttachedPropertyExpressionBody(string accessModifier)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);
    }
}";
            testCode = testCode.AssertReplace("    public", $"    {accessModifier}");
            AnalyzerAssert.Valid<WPF0004ClrMethodShouldMatchRegisteredName>(testCode);
        }

        [TestCase("public")]
        [TestCase("private")]
        public void ReadOnlyAttachedProperty(string accessModifier)
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

            public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);
    }
}";
            testCode = testCode.AssertReplace("    public", $"    {accessModifier}");
            AnalyzerAssert.Valid<WPF0004ClrMethodShouldMatchRegisteredName>(testCode);
        }

        [Test]
        public void IgnoresWhenSetMethodIsNotUsingValue()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SomeName(FrameworkElement element, int value)
        {
            element.SetValue(BarProperty, 1);
        }

        public static int GetBar(FrameworkElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

            AnalyzerAssert.Valid<WPF0004ClrMethodShouldMatchRegisteredName>(testCode);
        }

        [Test]
        public void IgnoresWhenSetMethodHasTooManyArguments()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SomeName(FrameworkElement element, int value, string text)
        {
            element.SetValue(BarProperty, 1);
        }

        public static int GetBar(FrameworkElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

            AnalyzerAssert.Valid<WPF0004ClrMethodShouldMatchRegisteredName>(testCode);
        }

        [Test]
        public void IgnoresWhenSetMethodIsNotVoid()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static object SomeName(FrameworkElement element, int value)
        {
            element.SetValue(BarProperty, value);
            return null;
        }

        public static int GetBar(FrameworkElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

            AnalyzerAssert.Valid<WPF0004ClrMethodShouldMatchRegisteredName>(testCode);
        }

        [Test]
        public void IgnoresWhenGetMethodIsVoid()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetBar(this FrameworkElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static void SomeName(FrameworkElement element)
        {
            var _ = (int)element.GetValue(BarProperty);
        }
    }
}";

            AnalyzerAssert.Valid<WPF0004ClrMethodShouldMatchRegisteredName>(testCode);
        }

        [Test]
        public void IgnoresWhenGetMethodHasTooManyArguments()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetBar(this FrameworkElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int SomeName(FrameworkElement element, int value)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

            AnalyzerAssert.Valid<WPF0004ClrMethodShouldMatchRegisteredName>(testCode);
        }

        [Test]
        public void IgnoresWhenSetMethodIsNotStatic()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public void SomeName(FrameworkElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int GetBar(FrameworkElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

            AnalyzerAssert.Valid<WPF0004ClrMethodShouldMatchRegisteredName>(testCode);
        }

        [Test]
        public void IgnoresWhenGetMethodIsNotStatic()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetBar(FrameworkElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public int SomeName(FrameworkElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

            AnalyzerAssert.Valid<WPF0004ClrMethodShouldMatchRegisteredName>(testCode);
        }

        [Test]
        public void IgnoresWhenSetMethodIsNotSettingElement()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        private static readonly FrameworkElement Element = new FrameworkElement();

        public static void Bar(FrameworkElement element, int value)
        {
            Element.SetValue(BarProperty, value);
        }

        public static int GetBar(FrameworkElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

            AnalyzerAssert.Valid<WPF0004ClrMethodShouldMatchRegisteredName>(testCode);
        }

        [Test]
        public void IgnoresWhenGetMethodIsNotGettingElement()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        private static readonly FrameworkElement Element = new FrameworkElement();

        public static void SetBar(FrameworkElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int Meh(FrameworkElement element)
        {
            return (int)Element.GetValue(BarProperty);
        }
    }
}";

            AnalyzerAssert.Valid<WPF0004ClrMethodShouldMatchRegisteredName>(testCode);
        }
    }
}