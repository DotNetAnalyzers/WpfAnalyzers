namespace WpfAnalyzers.Test.WPF0084XamlSetMarkupExtensionAttributeTargetTests
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

    [XamlSetMarkupExtension(nameof(ReceiveMarkupExtension))]
    public class WithSetMarkupExtensionAttribute : Control
    {
        public static void ReceiveMarkupExtension(object targetObject, XamlSetMarkupExtensionEventArgs eventArgs)
        {
        }
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }
    }
}
