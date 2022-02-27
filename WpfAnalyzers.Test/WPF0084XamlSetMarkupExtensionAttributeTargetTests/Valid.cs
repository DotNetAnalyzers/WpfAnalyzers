namespace WpfAnalyzers.Test.WPF0084XamlSetMarkupExtensionAttributeTargetTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly AttributeAnalyzer Analyzer = new();

    [Test]
    public static void WhenCorrectSignature()
    {
        var code = @"
namespace N
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
        RoslynAssert.Valid(Analyzer, code);
    }
}
