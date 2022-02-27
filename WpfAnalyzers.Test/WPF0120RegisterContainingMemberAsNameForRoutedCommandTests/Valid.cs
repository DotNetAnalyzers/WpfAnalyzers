namespace WpfAnalyzers.Test.WPF0120RegisterContainingMemberAsNameForRoutedCommandTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly RoutedCommandCreationAnalyzer Analyzer = new();

        [Test]
        public static void RoutedCommandNameOf()
        {
            var code = @"
namespace N
{
    using System.Windows.Input;

    public static class C
    {
        public static readonly RoutedCommand F = new RoutedCommand(nameof(F), typeof(C));
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("F")]
        [TestCase("FCommand")]
        public static void RoutedCommandLiteralName(string fieldName)
        {
            var code = @"
namespace N
{
    using System.Windows.Input;

    public static class C
    {
        public static readonly RoutedCommand F = new RoutedCommand(""F"", typeof(C));
    }
}".AssertReplace("public static readonly RoutedCommand F", $"public static readonly RoutedCommand {fieldName}");
            RoslynAssert.Valid(Analyzer, Descriptors.WPF0120RegisterContainingMemberAsNameForRoutedCommand, code);
        }

        [Test]
        public static void RoutedUICommand()
        {
            var code = @"
namespace N
{
    using System.Windows.Input;

    public static class C
    {
        public static readonly RoutedUICommand F = new RoutedUICommand(""Some text"", nameof(F), typeof(C));
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
