namespace WpfAnalyzers.Test.WPF0121RegisterContainingTypeAsOwnerForRoutedCommandTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly RoutedCommandCreationAnalyzer Analyzer = new();
        private static readonly UseContainingTypeFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0121RegisterContainingTypeAsOwnerForRoutedCommand);

        [Test]
        public static void RoutedCommand()
        {
            var before = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public static readonly RoutedCommand Bar = new RoutedCommand(nameof(Bar), ↓typeof(int));
    }
}";

            var after = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public static readonly RoutedCommand Bar = new RoutedCommand(nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void RoutedUICommand()
        {
            var before = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public static readonly RoutedUICommand Bar = new RoutedUICommand(""Some text"", nameof(Bar), ↓typeof(int));
    }
}";

            var after = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public static readonly RoutedUICommand Bar = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
