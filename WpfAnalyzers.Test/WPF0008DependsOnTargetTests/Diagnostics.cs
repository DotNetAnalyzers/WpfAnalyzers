namespace WpfAnalyzers.Test.WPF0008DependsOnTargetTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0008DependsOnTarget);

        [TestCase("[DependsOn(nameof(↓WithDependsOn))]")]
        [TestCase("[DependsOn(↓\"MISSING\")]")]
        public static void WhenMissing(string attribute)
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Markup;

    public class WithDependsOn : FrameworkElement
    {
        public static readonly DependencyProperty Value1Property = DependencyProperty.Register(
            nameof(Value1),
            typeof(string),
            typeof(WithDependsOn));

        public static readonly DependencyProperty Value2Property = DependencyProperty.Register(
            nameof(Value2),
            typeof(string),
            typeof(WithDependsOn));


#pragma warning disable WPF0150 // Use nameof().
        [DependsOn(↓""MISSING"")]
#pragma warning restore WPF0150 // Use nameof().
        public string Value1
        {
            get => (string)this.GetValue(Value1Property);
            set => this.SetValue(Value1Property, value);
        }

        public string Value2
        {
            get => (string)this.GetValue(Value2Property);
            set => this.SetValue(Value2Property, value);
        }
    }
}".AssertReplace("[DependsOn(↓\"MISSING\")]", attribute);
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
