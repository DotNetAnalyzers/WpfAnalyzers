namespace WpfAnalyzers.Test.WPF0171StyleTypedPropertyTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0171StyleTypedPropertyPropertyType);

        [Test]
        public static void WhenWrong()
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    [StyleTypedProperty(Property = nameof(↓BarStyle), StyleTargetType = typeof(Control))]
    public class WithStyleTypedProperty : Control
    {
        /// <summary>Identifies the <see cref=""BarStyle""/> dependency property.</summary>
        public static readonly DependencyProperty BarStyleProperty = DependencyProperty.Register(
            nameof(BarStyle),
            typeof(DataTemplate),
            typeof(WithStyleTypedProperty),
            new PropertyMetadata(default(DataTemplate)));

        public DataTemplate BarStyle
        {
            get => (DataTemplate)this.GetValue(BarStyleProperty);
            set => this.SetValue(BarStyleProperty, value);
        }
    }
}";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
