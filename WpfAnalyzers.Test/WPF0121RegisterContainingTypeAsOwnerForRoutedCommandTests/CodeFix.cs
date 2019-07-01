namespace WpfAnalyzers.Test.WPF0121RegisterContainingTypeAsOwnerForRoutedCommandTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new RoutedCommandCreationAnalyzer();
        private static readonly CodeFixProvider Fix = new UseContainingTypeFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0121RegisterContainingTypeAsOwnerForRoutedCommand.Descriptor);

        [Test]
        public static void RoutedCommand()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public static class Foo
    {
        public static readonly RoutedCommand Bar = new RoutedCommand(nameof(Bar), ↓typeof(int));
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public static class Foo
    {
        public static readonly RoutedCommand Bar = new RoutedCommand(nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public static void RoutedUICommand()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public static class Foo
    {
        public static readonly RoutedUICommand Bar = new RoutedUICommand(""Some text"", nameof(Bar), ↓typeof(int));
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public static class Foo
    {
        public static readonly RoutedUICommand Bar = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}
