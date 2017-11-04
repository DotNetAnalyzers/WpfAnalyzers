namespace WpfAnalyzers.Test.WPF0080MarkupExtensionDoesNotHaveAttributeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class CodeFix
    {
        [Test]
        public void Message()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Windows.Markup;

    public class ↓FooExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}";

            var expectedDiagnostic = ExpectedDiagnostic.CreateFromCodeWithErrorsIndicated(
                "WPF0080",
                "Add MarkupExtensionReturnType attribute.",
                testCode,
                out testCode);
            AnalyzerAssert.Diagnostics<WPF0080MarkupExtensionDoesNotHaveAttribute>(expectedDiagnostic, testCode);
        }

        [Test]
        public void DirectCastWrongSourceType()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Windows.Markup;

    public class ↓FooExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}";

            var fixedCode = @"
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
            AnalyzerAssert.CodeFix<WPF0080MarkupExtensionDoesNotHaveAttribute, AddMarkupExtensionReturnTypeAttributeFix>(testCode, fixedCode);
        }
    }
}