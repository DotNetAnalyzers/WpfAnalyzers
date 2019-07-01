namespace WpfAnalyzers.Test.WPF0140UseContainingTypeComponentResourceTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ComponentResourceKeyAnalyzer();
        private static readonly CodeFixProvider UseContainingTypeFix = new UseContainingTypeFix();
        private static readonly CodeFixProvider ComponentResourceKeyFix = new ComponentResourceKeyFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0140UseContainingTypeComponentResourceKey.Descriptor);

        [Test]
        public static void WhenNotContainingType()
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
            RoslynAssert.NoFix(Analyzer, ComponentResourceKeyFix, ExpectedDiagnostic, testCode);
            RoslynAssert.CodeFix(Analyzer, UseContainingTypeFix, ExpectedDiagnostic.WithMessage("Use containing type: ResourceKeys."), testCode, fixedCode);
        }

        [Test]
        public static void WhenNoArguments()
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
            RoslynAssert.NoFix(Analyzer, UseContainingTypeFix, ExpectedDiagnostic, testCode);
            RoslynAssert.CodeFix(Analyzer, ComponentResourceKeyFix, ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}
