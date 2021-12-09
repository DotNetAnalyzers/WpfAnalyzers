namespace WpfAnalyzers.Test.WPF0043DoNotUseSetCurrentValueForDataContextTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly SetValueAnalyzer Analyzer = new();
        private static readonly UseSetValueFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0043DoNotUseSetCurrentValue);

        [TestCase("this.SetCurrentValue(DataContextProperty, null);",                  "this.SetValue(DataContextProperty, null);")]
        [TestCase("this.SetCurrentValue(FrameworkElement.DataContextProperty, null);", "this.SetValue(FrameworkElement.DataContextProperty, null);")]
        [TestCase("SetCurrentValue(DataContextProperty, null);",                       "SetValue(DataContextProperty, null);")]
        [TestCase("SetCurrentValue(FrameworkElement.DataContextProperty, null);",      "SetValue(FrameworkElement.DataContextProperty, null);")]
        public static void ThisSetCurrentValueDataContextProperty(string statementBefore, string statementAfter)
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public FooControl()
        {
            ↓this.SetCurrentValue(DataContextProperty, null);
        }
    }
}".AssertReplace("this.SetCurrentValue(DataContextProperty, null);", statementBefore);

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public FooControl()
        {
            this.SetValue(DataContextProperty, null);
        }
    }
}".AssertReplace("this.SetValue(DataContextProperty, null);", statementAfter);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("control.SetCurrentValue(DataContextProperty, null);",                  "control.SetValue(DataContextProperty, null);")]
        [TestCase("control.SetCurrentValue(FrameworkElement.DataContextProperty, null);", "control.SetValue(FrameworkElement.DataContextProperty, null);")]
        public static void ControlSetCurrentValueDataContextProperty(string expressionBefore, string expressionAfter)
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static void Meh()
        {
            var control = new Control();
            ↓control.SetCurrentValue(FrameworkElement.DataContextProperty, null);
        }
    }
}".AssertReplace("control.SetCurrentValue(FrameworkElement.DataContextProperty, null);", expressionBefore);

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static void Meh()
        {
            var control = new Control();
            control.SetValue(FrameworkElement.DataContextProperty, null);
        }
    }
}".AssertReplace("control.SetValue(FrameworkElement.DataContextProperty, null);", expressionAfter);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("this.SetCurrentValue(DataContextProperty, null);",                  "this.SetValue(DataContextProperty, null);")]
        [TestCase("this.SetCurrentValue(FrameworkElement.DataContextProperty, null);", "this.SetValue(FrameworkElement.DataContextProperty, null);")]
        [TestCase("SetCurrentValue(DataContextProperty, null);",                       "SetValue(DataContextProperty, null);")]
        [TestCase("SetCurrentValue(FrameworkElement.DataContextProperty, null);",      "SetValue(FrameworkElement.DataContextProperty, null);")]
        public static void ThisSetCurrentValueStyleProperty(string statementBefore, string statementAfter)
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public FooControl()
        {
            ↓this.SetCurrentValue(StyleProperty, null);
        }
    }
}".AssertReplace("this.SetCurrentValue(StyleProperty, null);", statementBefore);

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public FooControl()
        {
            this.SetValue(StyleProperty, null);
        }
    }
}".AssertReplace("this.SetValue(StyleProperty, null);", statementAfter);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("control.SetCurrentValue(StyleProperty, null);",                  "control.SetValue(StyleProperty, null);")]
        [TestCase("control.SetCurrentValue(FrameworkElement.StyleProperty, null);", "control.SetValue(FrameworkElement.StyleProperty, null);")]
        public static void ControlSetCurrentValueStyleProperty(string expressionBefore, string expressionAfter)
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static void Meh()
        {
            var control = new Control();
            ↓control.SetCurrentValue(FrameworkElement.StyleProperty, null);
        }
    }
}".AssertReplace("control.SetCurrentValue(FrameworkElement.StyleProperty, null);", expressionBefore);

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static void Meh()
        {
            var control = new Control();
            control.SetValue(FrameworkElement.StyleProperty, null);
        }
    }
}".AssertReplace("control.SetValue(FrameworkElement.StyleProperty, null);", expressionAfter);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
