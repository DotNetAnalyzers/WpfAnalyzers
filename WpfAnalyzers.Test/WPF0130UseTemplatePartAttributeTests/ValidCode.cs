namespace WpfAnalyzers.Test.WPF0130UseTemplatePartAttributeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new GetTemplateChildAnalyzer();

        [Test]
        public void StringLiterals()
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
        public void Constant()
        {
            var testCode = @"
namespace RoslynSandbox
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void TemplatePartAttribute()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePartAttribute(Name = ""PART_Bar"", Type = typeof(Border))]
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
        public void BaseClassLiteral()
        {
            var baseCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = ""PART_Bar"", Type = typeof(Border))]
    public class BaseControl : Control
    {
    }
}";

            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Controls;

    public class FooControl : BaseControl
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = (Border)this.GetTemplateChild(""PART_Bar"");
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, baseCode, testCode);
            AnalyzerAssert.Valid(Analyzer, testCode, baseCode);
        }

        [Test]
        public void BaseClassConstant()
        {
            var baseCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = PartBar, Type = typeof(Border))]
    public class BaseControl : Control
    {
        protected const string PartBar = ""PART_Bar"";
    }
}";

            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Controls;

    public class FooControl : BaseControl
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var bar = (Border)this.GetTemplateChild(PartBar);
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, baseCode, testCode);
            AnalyzerAssert.Valid(Analyzer, testCode, baseCode);
        }

        [Test]
        public void IsPatternStringLiteral()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = PartBar, Type = typeof(Border))]
    public class FooControl : Control
    {
        private const string PartBar = ""PART_Bar"";

        private Border bar;

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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("as FrameworkElement")]
        [TestCase("as UIElement")]
        [TestCase("as Control")]
        public void AsCastStringLiteral(string cast)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    [TemplatePart(Name = PartBar, Type = typeof(FrameworkElement))]
    public class FooControl : Control
    {
        private const string PartBar = ""PART_Bar"";

        private object bar;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.bar = this.GetTemplateChild(PartBar) as FrameworkElement;
        }
    }
}".AssertReplace("as FrameworkElement", cast);

            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}
