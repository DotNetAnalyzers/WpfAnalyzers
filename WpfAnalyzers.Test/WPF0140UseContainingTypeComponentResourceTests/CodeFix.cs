namespace WpfAnalyzers.Test.WPF0140UseContainingTypeComponentResourceTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ComponentResourceKeyAnalyzer();
        private static readonly CodeFixProvider UseContainingTypeFix = new UseContainingTypeCodeFixProvider();
        private static readonly CodeFixProvider ComponentResourceKeyFix = new ComponentResourceKeyFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("WPF0140");

        [Test]
        public void WhenNotContainingType()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class ResourceKeys
    {
        public static readonly ComponentResourceKey FooKey = new ComponentResourceKey(
            ↓typeof(string),
            $""{typeof(ResourceKeys).FullName}.{nameof(FooKey)}"");
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class ResourceKeys
    {
        public static readonly ComponentResourceKey FooKey = new ComponentResourceKey(
            typeof(ResourceKeys),
            $""{typeof(ResourceKeys).FullName}.{nameof(FooKey)}"");
    }
}";
            AnalyzerAssert.NoFix(Analyzer, ComponentResourceKeyFix, ExpectedDiagnostic, testCode);
            AnalyzerAssert.CodeFix(Analyzer, UseContainingTypeFix, ExpectedDiagnostic.WithMessage("Use containing type: ResourceKeys."), testCode, fixedCode);
        }

        [Test]
        public void WhenNoArguments()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class ResourceKeys
    {
        public static readonly ComponentResourceKey FooKey = new ComponentResourceKey();
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class ResourceKeys
    {
        public static readonly ComponentResourceKey FooKey = new ComponentResourceKey(typeof(ResourceKeys), nameof(FooKey));
    }
}";
            AnalyzerAssert.NoFix(Analyzer, UseContainingTypeFix, ExpectedDiagnostic, testCode);
            AnalyzerAssert.CodeFix(Analyzer, ComponentResourceKeyFix, ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}
