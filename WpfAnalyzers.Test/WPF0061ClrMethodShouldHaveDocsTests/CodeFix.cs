namespace WpfAnalyzers.Test.WPF0061ClrMethodShouldHaveDocsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ClrMethodDeclarationAnalyzer();
        private static readonly CodeFixProvider Fix = new DocumentClrMethodCodeFixProvider();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("WPF0061");

        [Test]
        public void DependencyPropertyRegisterAttachedWithAttachedPropertyBrowsableForType()
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

        /// <summary>Helper for setting Bar property on <paramref name=""element""/>.</summary>
        /// <param name=""element"">UIElement to set Bar property on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static int ↓GetBar(UIElement element)
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
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting Bar property on <paramref name=""element""/>.</summary>
        /// <param name=""element"">UIElement to set Bar property on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for reading Bar property from <paramref name=""element""/>.</summary>
        /// <param name=""element"">UIElement to read Bar property from.</param>
        /// <returns>Bar property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
            AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void DependencyPropertyRegisterAttached()
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

        public static void ↓SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int ↓GetBar(UIElement element)
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
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting Bar property on <paramref name=""element""/>.</summary>
        /// <param name=""element"">UIElement to set Bar property on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for reading Bar property from <paramref name=""element""/>.</summary>
        /// <param name=""element"">UIElement to read Bar property from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

            AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void DependencyPropertyAddOwner()
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
            new FrameworkPropertyMetadata(
                default(int), 
                FrameworkPropertyMetadataOptions.Inherits));

        public static void ↓SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int ↓GetBar(UIElement element)
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
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int), 
            typeof(Foo), 
            new FrameworkPropertyMetadata(
                default(int), 
                FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>Helper for setting Bar property on <paramref name=""element""/>.</summary>
        /// <param name=""element"">UIElement to set Bar property on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for reading Bar property from <paramref name=""element""/>.</summary>
        /// <param name=""element"">UIElement to read Bar property from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

            AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}
