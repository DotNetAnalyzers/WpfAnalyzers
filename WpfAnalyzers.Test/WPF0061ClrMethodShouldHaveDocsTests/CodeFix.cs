namespace WpfAnalyzers.Test.WPF0061ClrMethodShouldHaveDocsTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly ClrMethodDeclarationAnalyzer Analyzer = new();
        private static readonly DocumentationFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0061DocumentClrMethod);

        [Test]
        public static void GetMissingDocs()
        {
            var before = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int ↓GetBar(UIElement element)
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
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void GetSummaryMissing()
        {
            var before = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        ///↓ <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
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
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void GetSummaryWrong()
        {
            var before = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>↓WRONG</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
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
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Ignore("tbd")]
        [Test]
        public static void GetSummaryWrongMultiline()
        {
            var before = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>
        /// ↓WRONG
        /// </summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
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
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void GetSummaryWrongCref()
        {
            var before = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""↓WRONG""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
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
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void GetSummaryWrongParamref()
        {
            var before = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""↓WRONG""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
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
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void GetParamMissing()
        {
            var before = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement ↓element)
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
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void GetParamWrong()
        {
            var before = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element"">↓WRONG.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
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
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void GetReturnsMissing()
        {
            var before = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        public static ↓int GetBar(UIElement element)
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
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void GetReturnsWrong()
        {
            var before = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>↓WRONG</returns>
        public static int GetBar(UIElement element)
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
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void SetMissingDocs()
        {
            var before = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int ↓GetBar(UIElement element)
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
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
