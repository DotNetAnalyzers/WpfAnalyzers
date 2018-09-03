namespace WpfAnalyzers.Test.WPF0131TemplatePartTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetTemplateChildAnalyzer();

        [Test]
        public void WhenCastingToSameType()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = ""PART_Bar"", Type = typeof(Border))]
    public class FooControl : Control
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = (Border)this.GetTemplateChild(""PART_Bar"");
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenCastingToSameTypeFullyQualified()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    [TemplatePart(Name = ""PART_Bar"", Type = typeof(System.Windows.Controls.Border))]
    public class FooControl : System.Windows.Controls.Control
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = (System.Windows.Controls.Border)this.GetTemplateChild(""PART_Bar"");
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}
