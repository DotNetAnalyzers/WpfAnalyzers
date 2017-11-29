namespace WpfAnalyzers.Test.WPF0033UseAttachedPropertyBrowsableForTypeAttributeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class HappyPath
    {
        private static readonly WPF0033UseAttachedPropertyBrowsableForTypeAttribute Analyzer =
            new WPF0033UseAttachedPropertyBrowsableForTypeAttribute();

        [Test]
        public void WhenHasAttribute()
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

        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static int GetValue(DependencyObject element)
        {
            return (int)element.GetValue(ValueProperty);
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}