namespace WpfAnalyzers.Test.WPF0140UseContainingTypeComponentResourceTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class CodeFix
{
    private static readonly ComponentResourceKeyAnalyzer Analyzer = new();
    private static readonly UseContainingTypeFix UseContainingTypeFix = new();
    private static readonly ComponentResourceKeyFix ComponentResourceKeyFix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0140UseContainingTypeComponentResourceKey);

    [Test]
    public static void Message()
    {
        var before = @"
namespace N
{
    using System.Windows;

    public static class ResourceKeys
    {
        public static readonly ComponentResourceKey FooKey = new ComponentResourceKey(
            ↓typeof(string),
            $""{typeof(ResourceKeys).FullName}.{nameof(FooKey)}"");
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
            $""{typeof(ResourceKeys).FullName}.{nameof(FooKey)}"");
    }
}";
        RoslynAssert.NoFix(Analyzer, ComponentResourceKeyFix, ExpectedDiagnostic, before);
        RoslynAssert.CodeFix(Analyzer, UseContainingTypeFix, ExpectedDiagnostic.WithMessage("Use containing type: ResourceKeys"), before, after);
    }

    [Test]
    public static void WhenNotContainingType()
    {
        var before = @"
namespace N
{
    using System.Windows;

    public static class ResourceKeys
    {
        public static readonly ComponentResourceKey FooKey = new ComponentResourceKey(
            ↓typeof(string),
            $""{typeof(ResourceKeys).FullName}.{nameof(FooKey)}"");
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
            $""{typeof(ResourceKeys).FullName}.{nameof(FooKey)}"");
    }
}";
        RoslynAssert.NoFix(Analyzer, ComponentResourceKeyFix, ExpectedDiagnostic, before);
        RoslynAssert.CodeFix(Analyzer, UseContainingTypeFix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenNoArguments()
    {
        var before = @"
namespace N
{
    using System.Windows;

    public static class ResourceKeys
    {
        public static readonly ComponentResourceKey FooKey = new ComponentResourceKey↓();
    }
}";

        var after = @"
namespace N
{
    using System.Windows;

    public static class ResourceKeys
    {
        public static readonly ComponentResourceKey FooKey = new ComponentResourceKey(typeof(ResourceKeys), nameof(FooKey));
    }
}";
        RoslynAssert.NoFix(Analyzer, UseContainingTypeFix, ExpectedDiagnostic, before);
        RoslynAssert.CodeFix(Analyzer, ComponentResourceKeyFix, ExpectedDiagnostic, before, after);
    }
}