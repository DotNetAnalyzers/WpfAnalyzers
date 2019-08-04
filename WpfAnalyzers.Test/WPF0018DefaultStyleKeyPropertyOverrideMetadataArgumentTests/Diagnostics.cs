namespace WpfAnalyzers.Test.WPF0018DefaultStyleKeyPropertyOverrideMetadataArgumentTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new OverrideMetadataAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0018DefaultStyleKeyPropertyOverrideMetadataArgument);

        [Test]
        public static void DefaultStyleKeyPropertyOverrideMetadata()
        {
            var testCode = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        static FooControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FooControl), new FrameworkPropertyMetadata(↓typeof(Control)));
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }
    }
}
