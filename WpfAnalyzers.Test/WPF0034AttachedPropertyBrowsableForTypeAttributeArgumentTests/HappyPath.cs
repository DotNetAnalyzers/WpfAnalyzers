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

        /// <summary>Helper for setting <see cref=""ValueProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to set <see cref=""ValueProperty""/> on.</param>
        /// <param name=""value"">Value property value.</param>
        public static void SetValue(DependencyObject element, int value)
        {
            element.SetValue(ValueProperty, value);
        }

        /// <summary>Helper for getting <see cref=""ValueProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to read <see cref=""ValueProperty""/> from.</param>
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

        [Test]
        public void WhenHasAttributeForMoreSpecificType()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class Foo
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
            ""Value"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""ValueProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to set <see cref=""ValueProperty""/> on.</param>
        /// <param name=""value"">Value property value.</param>
        public static void SetValue(DependencyObject element, int value)
        {
            element.SetValue(ValueProperty, value);
        }

        /// <summary>Helper for getting <see cref=""ValueProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to read <see cref=""ValueProperty""/> from.</param>
        /// <returns>Value property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static int GetValue(DependencyObject element)
        {
            return (int)element.GetValue(ValueProperty);
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenHasTwoAttributesForMoreSpecificTypes()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class Foo
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
            ""Value"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for setting <see cref=""ValueProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to set <see cref=""ValueProperty""/> on.</param>
        /// <param name=""value"">Value property value.</param>
        public static void SetValue(DependencyObject element, int value)
        {
            element.SetValue(ValueProperty, value);
        }

        /// <summary>Helper for getting <see cref=""ValueProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to read <see cref=""ValueProperty""/> from.</param>
        /// <returns>Value property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
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
