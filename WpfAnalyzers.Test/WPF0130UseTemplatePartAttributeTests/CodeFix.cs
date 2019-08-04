namespace WpfAnalyzers.Test.WPF0130UseTemplatePartAttributeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetTemplateChildAnalyzer();
        private static readonly CodeFixProvider Fix = new AddTemplatePartAttributeFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0130UseTemplatePartAttribute);

        [Test]
        public static void StringLiteralNoCast()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = ↓this.GetTemplateChild(""PART_Bar"");
        }
    }
}";

            var after = @"
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
            var bar = this.GetTemplateChild(""PART_Bar"");
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void CastStringLiteral()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = (Border)↓this.GetTemplateChild(""PART_Bar"");
        }
    }
}";

            var after = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void IsPatternStringLiteral()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private Border bar;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.bar = null;
            if (↓this.GetTemplateChild(""PART_Bar"") is Border border)
            {
                this.bar = border;
            }
        }
    }
}";

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = ""PART_Bar"", Type = typeof(Border))]
    public class FooControl : Control
    {
        private Border bar;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.bar = null;
            if (this.GetTemplateChild(""PART_Bar"") is Border border)
            {
                this.bar = border;
            }
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void CastConstant()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private const string PartBar = ""PART_Bar"";

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = (Border)↓this.GetTemplateChild(PartBar);
        }
    }
}";

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = PartBar, Type = typeof(Border))]
    public class FooControl : Control
    {
        private const string PartBar = ""PART_Bar"";

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = (Border)this.GetTemplateChild(PartBar);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void CastNameof()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = (Border)↓this.GetTemplateChild(nameof(OnApplyTemplate));
        }
    }
}";

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = nameof(OnApplyTemplate), Type = typeof(Border))]
    public class FooControl : Control
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = (Border)this.GetTemplateChild(nameof(OnApplyTemplate));
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenWrongAttributeCastStringLiteral()
        {
            var before = @"
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
            var bar = (Border)↓this.GetTemplateChild(""PART_Baz"");
        }
    }
}";

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = ""PART_Bar"", Type = typeof(Border))]
    [TemplatePart(Name = ""PART_Baz"", Type = typeof(Border))]
    public class FooControl : Control
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = (Border)this.GetTemplateChild(""PART_Baz"");
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
