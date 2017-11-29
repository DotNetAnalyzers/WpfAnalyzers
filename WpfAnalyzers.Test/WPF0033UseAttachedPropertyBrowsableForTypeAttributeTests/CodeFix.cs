namespace WpfAnalyzers.Test.WPF0033UseAttachedPropertyBrowsableForTypeAttributeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class CodeFix
    {
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

        public static void SetValue(DependencyObject element, int value)
        {
            element.SetValue(ValueProperty, value);
        }

        public static int ↓GetValue(DependencyObject element)
        {
            return (int)element.GetValue(ValueProperty);
        }
    }
}";

            var expectedDiagnostic = ExpectedDiagnostic.CreateFromCodeWithErrorsIndicated(
                "WPF0033",
                "Add [AttachedPropertyBrowsableForType(typeof(DependencyObject)]",
                testCode,
                out testCode);
            AnalyzerAssert.Diagnostics<WPF0033UseAttachedPropertyBrowsableForTypeAttribute>(expectedDiagnostic, testCode);
        }

        [Test]
        public void AddAttribute()
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

        public static void SetValue(DependencyObject element, int value)
        {
            element.SetValue(ValueProperty, value);
        }

        public static int ↓GetValue(DependencyObject element)
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

        public static void SetValue(DependencyObject element, int value)
        {
            element.SetValue(ValueProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static int GetValue(DependencyObject element)
        {
            return (int)element.GetValue(ValueProperty);
        }
    }
}";

            AnalyzerAssert.CodeFix<WPF0033UseAttachedPropertyBrowsableForTypeAttribute, AttachedPropertyBrowsableForTypeAttributeFix>(testCode, fixedCode);
        }
    }
}
