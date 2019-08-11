namespace WpfAnalyzers.Test.WPF0008DependsOnTargetTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();

        [TestCase("[DependsOn(nameof(Value2))]")]
        [TestCase("[DependsOn(\"Value2\")]")]
        public static void WhenPropertyExists(string attribute)
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

        [DependsOn(nameof(Value2))]
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
}".AssertReplace("[DependsOn(nameof(Value2))]", attribute);
            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
