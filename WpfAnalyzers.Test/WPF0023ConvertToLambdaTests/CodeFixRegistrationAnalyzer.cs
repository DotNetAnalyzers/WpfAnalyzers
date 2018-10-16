namespace WpfAnalyzers.Test.WPF0023ConvertToLambdaTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class CodeFixRegistrationAnalyzer
    {
        private static readonly DiagnosticAnalyzer Analyzer = new RegistrationAnalyzer();
        private static readonly CodeFixProvider Fix = new ConvertToLambdaFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("WPF0023");

        [TestCase("ValidateValue", "value => (int)value >= 0")]
        [TestCase("x => ValidateValue(x)", "value => (int)value >= 0")]
        [TestCase("new ValidateValueCallback(ValidateValue)", "new ValidateValueCallback(value => (int)value >= 0)")]
        [TestCase("new ValidateValueCallback(x => ValidateValue(x))", "new ValidateValueCallback(value => (int)value >= 0)")]
        public void RemoveMethod(string callback, string lambda)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)),
            ↓ValidateValue);

        public int Value
        {
            get => (int)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        private static bool ValidateValue(object value)
        {
            return (int)value >= 0;
        }
    }
}".AssertReplace("ValidateValue);", $"{callback});");

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)),
            value => (int)value >= 0);

        public int Value
        {
            get => (int)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }
    }
}".AssertReplace("value => (int)value >= 0", lambda);

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "Convert to lambda");
        }
    }
}
