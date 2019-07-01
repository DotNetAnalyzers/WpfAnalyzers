namespace WpfAnalyzers.Test.WPF0081MarkupExtensionReturnTypeMustUseCorrectTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();

        [Test]
        public static void WhenHasAttribute()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(FooExtension))]
    public class FooExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void WhenReturnTypeIsObjectAndAttributeIsMoreSpecific()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Windows.Data;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(BindingExpression))]
    public class FooExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new Binding();
            return binding.ProvideValue(serviceProvider);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }
    }
}
