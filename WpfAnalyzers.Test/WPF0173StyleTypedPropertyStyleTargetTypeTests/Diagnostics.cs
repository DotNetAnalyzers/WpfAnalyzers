namespace WpfAnalyzers.Test.WPF0173StyleTypedPropertyStyleTargetTypeTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Diagnostics
{
    private static readonly AttributeAnalyzer Analyzer = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0173StyleTypedPropertyStyleTargetType);

    [Test]
    public static void WhenMissing()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    [StyleTypedProperty(Property = nameof(BarStyle),StyleTargetType = typeof(↓string))]
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
        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }
}
