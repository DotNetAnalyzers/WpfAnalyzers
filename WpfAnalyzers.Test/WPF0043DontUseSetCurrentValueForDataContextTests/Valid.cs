namespace WpfAnalyzers.Test.WPF0043DontUseSetCurrentValueForDataContextTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly SetValueAnalyzer Analyzer = new SetValueAnalyzer();

        [Test]
        public static void IgnoreSetDataContext()
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public FooControl()
        {
            this.DataContext = 1;
            DataContext = 1;
        }

        public static void Meh()
        {
            var control = new Control();
            control.SetValue(FrameworkElement.DataContextProperty, 1);
            control.SetCurrentValue(Control.FontSizeProperty, 12.0);
            control.DataContext = 1;
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
