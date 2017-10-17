namespace WpfAnalyzers.Test.WPF0043DontUseSetCurrentValueForDataContextTests
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    internal class HappyPath : HappyPathVerifier<WPF0043DontUseSetCurrentValueForDataContext>
    {
        [Test]
        public async Task IgnoreSetDataContext()
        {
            var testCode = @"
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
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }
    }
}