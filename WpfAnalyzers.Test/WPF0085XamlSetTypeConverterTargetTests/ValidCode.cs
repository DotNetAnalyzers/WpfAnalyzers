namespace WpfAnalyzers.Test.WPF0085XamlSetTypeConverterTargetTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();

        [Test]
        public static void WhenCorrectSignature()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Controls;
    using System.Windows.Markup;

    [XamlSetTypeConverter(nameof(ReceiveTypeConverter))]
    public class FooControl : Control
    {
        public static void ReceiveTypeConverter(object targetObject, XamlSetTypeConverterEventArgs eventArgs)
        {
        }
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }
    }
}
