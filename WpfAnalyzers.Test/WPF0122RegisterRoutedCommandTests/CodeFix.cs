namespace WpfAnalyzers.Test.WPF0122RegisterRoutedCommandTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new RoutedCommandCreationAnalyzer();
        private static readonly CodeFixProvider Fix = new RegisterRoutedCommandFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0122RegisterRoutedCommand.Descriptor);

        [Test]
        public void RoutedCommand()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public static class Foo
    {
        public static readonly RoutedCommand Bar = new RoutedCommand↓();
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
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void RoutedUICommand()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public static class Foo
    {
        public static readonly RoutedUICommand Bar = new RoutedUICommand↓();
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public static class Foo
    {
        public static readonly RoutedUICommand Bar = new RoutedUICommand(""PLACEHOLDER TEXT"", nameof(Bar), typeof(Foo));
    }
}";
            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}
