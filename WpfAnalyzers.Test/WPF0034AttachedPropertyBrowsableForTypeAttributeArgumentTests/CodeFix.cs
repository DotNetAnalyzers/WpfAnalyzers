namespace WpfAnalyzers.Test.WPF0034AttachedPropertyBrowsableForTypeAttributeArgumentTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class CodeFix
    {
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("WPF0034");

        [Test]
        public void Message()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public class Foo
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
            ""Value"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetValue(UIElement element, int value)
        {
            element.SetValue(ValueProperty, value);
        }

        [AttachedPropertyBrowsableForType(↓typeof(DependencyObject))]
        public static int GetValue(UIElement element)
        {
            return (int)element.GetValue(ValueProperty);
        }
    }
}";

            var expectedDiagnostic = ExpectedDiagnostic.Create(
                "WPF0034",
                "Use [AttachedPropertyBrowsableForType(typeof(UIElement)]");
            AnalyzerAssert.Diagnostics<ClrMethodDeclarationAnalyzer>(expectedDiagnostic, testCode);
        }

        [Test]
        public void WhenWrongType()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public class Foo
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
            ""Value"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetValue(UIElement element, int value)
        {
            element.SetValue(ValueProperty, value);
        }

        [AttachedPropertyBrowsableForType(↓typeof(DependencyObject))]
        public static int GetValue(UIElement element)
        {
            return (int)element.GetValue(ValueProperty);
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public class Foo
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
            ""Value"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetValue(UIElement element, int value)
        {
            element.SetValue(ValueProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static int GetValue(UIElement element)
        {
            return (int)element.GetValue(ValueProperty);
        }
    }
}";

            AnalyzerAssert.CodeFix<ClrMethodDeclarationAnalyzer, AttachedPropertyBrowsableForTypeArgumentFix>(ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}
