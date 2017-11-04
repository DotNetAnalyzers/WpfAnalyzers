namespace WpfAnalyzers.Test.WPF0081MarkupExtensionReturnTypeMustUseCorrectTypeTests
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

    [MarkupExtensionReturnType(↓typeof(int))]
    public class FooExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}";

            var expectedDiagnostic = ExpectedDiagnostic.CreateFromCodeWithErrorsIndicated(
                "WPF0081",
                "MarkupExtensionReturnType must use correct return type. Expected: RoslynSandbox.FooExtension",
                testCode,
                out testCode);
            AnalyzerAssert.Diagnostics<WPF0081MarkupExtensionReturnTypeMustUseCorrectType>(expectedDiagnostic, testCode);
        }

        [Test]
        public void ReturnThis()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(↓typeof(int))]
    public class FooExtension : MarkupExtension
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
            AnalyzerAssert.CodeFix<WPF0081MarkupExtensionReturnTypeMustUseCorrectType, ChangeMarkupExtensionReturnTypeFix>(testCode, fixedCode);
        }
    }
}