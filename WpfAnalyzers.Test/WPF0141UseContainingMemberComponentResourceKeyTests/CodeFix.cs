namespace WpfAnalyzers.Test.WPF0141UseContainingMemberComponentResourceKeyTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ComponentResourceKeyAnalyzer();
        private static readonly CodeFixProvider Fix = new ComponentResourceKeyFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0141UseContainingMemberComponentResourceKey.Descriptor);

        [Test]
        public static void Message()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class ResourceKeys
    {
        public static readonly ComponentResourceKey FooKey = new ComponentResourceKey(
            typeof(string),
            ↓nameof(ResourceKeys));
    }
}";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Use containing member: nameof(FooKey)."), testCode);
        }

        [Test]
        public static void WhenNotUsingNameofContainingMember()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class ResourceKeys
    {
        public static readonly ComponentResourceKey FooKey = new ComponentResourceKey(
            typeof(ResourceKeys),
            ↓nameof(ResourceKeys));
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class ResourceKeys
    {
        public static readonly ComponentResourceKey FooKey = new ComponentResourceKey(
            typeof(ResourceKeys),
            nameof(FooKey));
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenUsingWrongStringLiteral()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class ResourceKeys
    {
        public static readonly ComponentResourceKey FooKey = new ComponentResourceKey(
            typeof(ResourceKeys),
            ↓""WrongName"");
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class ResourceKeys
    {
        public static readonly ComponentResourceKey FooKey = new ComponentResourceKey(
            typeof(ResourceKeys),
            nameof(FooKey));
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
