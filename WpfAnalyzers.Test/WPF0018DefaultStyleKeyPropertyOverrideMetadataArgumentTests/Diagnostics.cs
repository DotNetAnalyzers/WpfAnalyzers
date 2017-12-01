namespace WpfAnalyzers.Test.WPF0018DefaultStyleKeyPropertyOverrideMetadataArgumentTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("WPF0018");

        [Test]
        public void DefaultStyleKeyPropertyOverrideMetadata()
        {
            var testCode = @"
namespace RoslynSandbox
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

            AnalyzerAssert.Diagnostics<OverrideMetadataAnalyzer>(ExpectedDiagnostic, testCode);
        }
    }
}