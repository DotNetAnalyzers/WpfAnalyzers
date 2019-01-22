namespace WpfAnalyzers.Test.WPF0043DontUseSetCurrentValueForDataContextTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new SetValueAnalyzer();
        private static readonly CodeFixProvider Fix = new UseSetValueFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0043DontUseSetCurrentValueForDataContext.Descriptor);

        [TestCase("this.SetCurrentValue(DataContextProperty, 1);", "this.SetValue(DataContextProperty, 1);")]
        [TestCase("this.SetCurrentValue(FrameworkElement.DataContextProperty, 1);", "this.SetValue(FrameworkElement.DataContextProperty, 1);")]
        [TestCase("SetCurrentValue(DataContextProperty, 1);", "SetValue(DataContextProperty, 1);")]
        [TestCase("SetCurrentValue(FrameworkElement.DataContextProperty, 1);", "SetValue(FrameworkElement.DataContextProperty, 1);")]
        public void ThisSetCurrentValue(string before, string after)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public FooControl()
        {
            ↓this.SetCurrentValue(DataContextProperty, 1);
        }
    }
}".AssertReplace("this.SetCurrentValue(DataContextProperty, 1);", before);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public FooControl()
        {
            this.SetValue(DataContextProperty, 1);
        }
    }
}".AssertReplace("this.SetValue(DataContextProperty, 1);", after);

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [TestCase("control.SetCurrentValue(DataContextProperty, 1);", "control.SetValue(DataContextProperty, 1);")]
        [TestCase("control.SetCurrentValue(FrameworkElement.DataContextProperty, 1);", "control.SetValue(FrameworkElement.DataContextProperty, 1);")]
        public void ControlSetCurrentValue(string before, string after)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static void Meh()
        {
            var control = new Control();
            ↓control.SetCurrentValue(FrameworkElement.DataContextProperty, 1);
        }
    }
}".AssertReplace("control.SetCurrentValue(FrameworkElement.DataContextProperty, 1);", before);

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static void Meh()
        {
            var control = new Control();
            control.SetValue(FrameworkElement.DataContextProperty, 1);
        }
    }
}".AssertReplace("control.SetValue(FrameworkElement.DataContextProperty, 1);", after);

            AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}
