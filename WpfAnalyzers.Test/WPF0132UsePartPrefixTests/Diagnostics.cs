namespace WpfAnalyzers.Test.WPF0132UsePartPrefixTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly AttributeAnalyzer Analyzer = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0132UsePartPrefix);

        [Test]
        public static void MissingPrefix()
        {
            var code = @"
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
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Use PART prefix."), code);
        }
    }
}
