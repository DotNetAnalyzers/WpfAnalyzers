namespace WpfAnalyzers.Test.WPF0132UsePartPrefixTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0132UsePartPrefix);

        [Test]
        public static void MissingPrefix()
        {
            var testCode = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = ↓""Bar"", Type = typeof(Button))]
    public class FooControl : Control
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = (Border)this.GetTemplateChild(""Bar"");
        }
    }
}";
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Use PART prefix."), testCode);
        }
    }
}
