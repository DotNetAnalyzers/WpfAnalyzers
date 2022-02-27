namespace WpfAnalyzers.Test.WPF0121RegisterContainingTypeAsOwnerForRoutedCommandTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly RoutedCommandCreationAnalyzer Analyzer = new();

    [Test]
    public static void RoutedCommand()
    {
        var code = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public static readonly RoutedCommand Bar = new RoutedCommand(nameof(Bar), typeof(Foo));
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void RoutedUICommand()
    {
        var code = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public static readonly RoutedUICommand Bar = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }
}
