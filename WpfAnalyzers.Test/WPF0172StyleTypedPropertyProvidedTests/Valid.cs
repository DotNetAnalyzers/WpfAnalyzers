namespace WpfAnalyzers.Test.WPF0172StyleTypedPropertyProvidedTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.WPF0172StyleTypedPropertyPropertySpecified;

        [Test]
        public static void WhenExists()
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    [StyleTypedProperty(Property = nameof(BarStyle), StyleTargetType = typeof(Control))]
    public class WithStyleTypedProperty : Control
    {
        /// <summary>Identifies the <see cref=""BarStyle""/> dependency property.</summary>
        public static readonly DependencyProperty BarStyleProperty = DependencyProperty.Register(
            nameof(BarStyle),
            typeof(Style),
            typeof(WithStyleTypedProperty),
            new PropertyMetadata(default(Style)));

        public Style BarStyle
        {
            get => (Style)this.GetValue(BarStyleProperty);
            set => this.SetValue(BarStyleProperty, value);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
