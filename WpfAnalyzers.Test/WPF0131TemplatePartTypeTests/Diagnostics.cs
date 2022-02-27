namespace WpfAnalyzers.Test.WPF0131TemplatePartTypeTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Diagnostics
{
    private static readonly GetTemplateChildAnalyzer Analyzer = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0131TemplatePartType);

    [Test]
    public static void CastNotMatching()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = ""PART_Bar"", Type = typeof(Button))]
    public class FooControl : Control
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = (Border)↓this.GetTemplateChild(""PART_Bar"");
        }
    }
}";
        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Use correct [TemplatePart] type"), code);
    }

    [Test]
    public static void IsPatternNotMatching()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = PartBar, Type = typeof(Border))]
    public class FooControl : Control
    {
        private const string PartBar = ""PART_Bar"";

        private Button? bar;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.bar = null;
            if (↓this.GetTemplateChild(PartBar) is Button button)
            {
                this.bar = button;
            }
        }
    }
}";
        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }

    [Test]
    public static void AsCastNotMatching()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = PartBar, Type = typeof(Border))]
    public class FooControl : Control
    {
        private const string PartBar = ""PART_Bar"";

        private Button? bar;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.bar = ↓this.GetTemplateChild(PartBar) as Button;
        }
    }
}";
        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }

    [Test]
    public static void WhenMissingType()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = ""PART_Bar"")]
    public class FooControl : Control
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = (Border)↓this.GetTemplateChild(""PART_Bar"");
        }
    }
}";
        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }
}