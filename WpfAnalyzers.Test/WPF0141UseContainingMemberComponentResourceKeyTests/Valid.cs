namespace WpfAnalyzers.Test.WPF0141UseContainingMemberComponentResourceKeyTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly ComponentResourceKeyAnalyzer Analyzer = new();

    [Test]
    public static void WhenExpectedLiteral()
    {
        var code = @"
namespace N
{
    using System.Windows;

    public static class ResourceKeys
    {
        public static readonly ComponentResourceKey FooKey = new ComponentResourceKey(
            typeof(ResourceKeys),
            ""FooKey"");
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void WhenExpectedNameof()
    {
        var code = @"
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
        RoslynAssert.Valid(Analyzer, code);
    }
}
