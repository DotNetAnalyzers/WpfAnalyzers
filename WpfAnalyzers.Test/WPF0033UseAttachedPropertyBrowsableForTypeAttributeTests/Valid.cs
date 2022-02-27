﻿namespace WpfAnalyzers.Test.WPF0033UseAttachedPropertyBrowsableForTypeAttributeTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly ClrMethodDeclarationAnalyzer Analyzer = new();

    [Test]
    public static void WhenHasAttribute()
    {
        var code = @"
namespace N
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

        RoslynAssert.Valid(Analyzer, code);
    }
}
