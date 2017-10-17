namespace WpfAnalyzers.Test.WPF0043DontUseSetCurrentValueForDataContextTests
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    internal class CodeFix : CodeFixVerifier<WPF0043DontUseSetCurrentValueForDataContext, UseSetValueCodeFixProvider>
    {
        [TestCase("this.SetCurrentValue(DataContextProperty, 1);", "this.SetValue(DataContextProperty, 1);")]
        [TestCase("this.SetCurrentValue(FrameworkElement.DataContextProperty, 1);", "this.SetValue(FrameworkElement.DataContextProperty, 1);")]
        [TestCase("SetCurrentValue(DataContextProperty, 1);", "SetValue(DataContextProperty, 1);")]
        [TestCase("SetCurrentValue(FrameworkElement.DataContextProperty, 1);", "SetValue(FrameworkElement.DataContextProperty, 1);")]
        public async Task ThisSetCurrentValue(string before, string after)
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public FooControl()
    {
        ↓this.SetCurrentValue(DataContextProperty, 1);
    }
}";
            testCode = testCode.AssertReplace("this.SetCurrentValue(DataContextProperty, 1);", before);
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("DataContextProperty", "1");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public FooControl()
    {
        this.SetValue(DataContextProperty, 1);
    }
}";
            fixedCode = fixedCode.AssertReplace("this.SetValue(DataContextProperty, 1);", after);
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }

        [TestCase("control.SetCurrentValue(DataContextProperty, 1);", "control.SetValue(DataContextProperty, 1);")]
        [TestCase("control.SetCurrentValue(FrameworkElement.DataContextProperty, 1);", "control.SetValue(FrameworkElement.DataContextProperty, 1);")]
        public async Task ControlSetCurrentValue(string before, string after)
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static void Meh()
    {
        var control = new Control();
        ↓control.SetCurrentValue(FrameworkElement.DataContextProperty, 1);
    }
}";
            testCode = testCode.AssertReplace("control.SetCurrentValue(FrameworkElement.DataContextProperty, 1);", before);
            var expected = this.CSharpDiagnostic().WithLocationIndicated(ref testCode).WithArguments("DataContextProperty", "1");
            await this.VerifyCSharpDiagnosticAsync(testCode, expected).ConfigureAwait(false);

            var fixedCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static void Meh()
    {
        var control = new Control();
        control.SetValue(FrameworkElement.DataContextProperty, 1);
    }
}";
            fixedCode = fixedCode.AssertReplace("control.SetValue(FrameworkElement.DataContextProperty, 1);", after);
            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
        }
    }
}