namespace WpfAnalyzers.Test.WPF0141UseContainingMemberComponentResourceKeyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly ComponentResourceKeyAnalyzer Analyzer = new();
        private static readonly ComponentResourceKeyFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0141UseContainingMemberComponentResourceKey);

        [Test]
        public static void Message()
        {
            var code = @"
namespace N
{
    using System.Windows;

    public static class ResourceKeys
    {
        public static readonly ComponentResourceKey FooKey = new ComponentResourceKey(
            typeof(string),
            ↓nameof(ResourceKeys));
    }
}";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Use containing member: nameof(FooKey)."), code);
        }

        [Test]
        public static void WhenNotUsingNameofContainingMember()
        {
            var before = @"
namespace N
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
namespace N
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
namespace N
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
namespace N
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
