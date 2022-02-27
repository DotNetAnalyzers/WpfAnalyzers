namespace WpfAnalyzers.Test.WPF0170StyleTypedPropertyPropertyTargetTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Diagnostics
{
    private static readonly AttributeAnalyzer Analyzer = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0170StyleTypedPropertyPropertyTarget);

    [TestCase("[StyleTypedProperty(Property = ↓\"MISSING\", StyleTargetType = typeof(Control))]")]
    [TestCase("[StyleTypedProperty(Property = nameof(↓WithStyleTypedProperty), StyleTargetType = typeof(Control))]")]
    [TestCase("[StyleTypedProperty(Property = ↓WrongName, StyleTargetType = typeof(Control))]")]
    public static void WhenWrong(string attribute)
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    [StyleTypedProperty(Property = ↓""MISSING"", StyleTargetType = typeof(Control))]
    public class WithStyleTypedProperty : Control
    {
        const string WrongName = nameof(WithStyleTypedProperty);

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
}".AssertReplace("[StyleTypedProperty(Property = ↓\"MISSING\", StyleTargetType = typeof(Control))]", attribute);
        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }
}