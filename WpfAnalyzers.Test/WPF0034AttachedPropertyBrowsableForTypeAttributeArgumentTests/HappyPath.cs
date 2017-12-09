namespace WpfAnalyzers.Test.WPF0034AttachedPropertyBrowsableForTypeAttributeArgumentTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class HappyPath
    {
        private static readonly ClrMethodDeclarationAnalyzer Analyzer = new ClrMethodDeclarationAnalyzer();

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

        /// <summary>
        /// Helper for setting Bar property on a DependencyObject.
        /// </summary>
        /// <param name=""element"">DependencyObject to set Value property on.</param>
        /// <param name=""value"">Value property value.</param>
        public static void SetValue(DependencyObject element, int value)
        {
            element.SetValue(ValueProperty, value);
        }

         /// <summary>
        /// Helper for reading Bar property from a DependencyObject.
        /// </summary>
        /// <param name=""element"">DependencyObject to read Value property from.</param>
        /// <returns>Value property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
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