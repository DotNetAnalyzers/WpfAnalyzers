namespace WpfAnalyzers.Test.WPF0131TemplatePartTypeTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly GetTemplateChildAnalyzer Analyzer = new();

    [Test]
    public static void WhenCastingToSameType()
    {
        var code = @"
namespace N
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
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void WhenCastingToSameTypeFullyQualified()
    {
        var code = @"
namespace N
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
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void WhenCastingToLessSpecificType()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = ""PART_Bar"", Type = typeof(Border))]
    public class FooControl : Control
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = (FrameworkElement)this.GetTemplateChild(""PART_Bar"");
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void WhenIsPatternSameType()
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

        private Border? bar;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.bar = null;
            if (this.GetTemplateChild(PartBar) is Border border)
            {
                this.bar = border;
            }
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [TestCase("as FrameworkElement")]
    [TestCase("as UIElement")]
    [TestCase("as Control")]
    public static void AsCastStringLiteral(string cast)
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = PartBar, Type = typeof(FrameworkElement))]
    public class FooControl : Control
    {
        private const string PartBar = ""PART_Bar"";

        private object? bar;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.bar = this.GetTemplateChild(PartBar) as FrameworkElement;
        }
    }
}".AssertReplace("as FrameworkElement", cast);

        RoslynAssert.Valid(Analyzer, code);
    }
}