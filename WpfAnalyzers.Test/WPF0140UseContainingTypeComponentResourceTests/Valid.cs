namespace WpfAnalyzers.Test.WPF0140UseContainingTypeComponentResourceTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly ComponentResourceKeyAnalyzer Analyzer = new();

        [Test]
        public static void WhenExpectedArguments()
        {
            var code = @"
namespace N
{
    using System.Windows;

    public static class ResourceKeys
    {
        public static readonly ComponentResourceKey FooKey = new ComponentResourceKey(
            typeof(ResourceKeys),
            $""{typeof(ResourceKeys).FullName}.{nameof(FooKey)}"");
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
