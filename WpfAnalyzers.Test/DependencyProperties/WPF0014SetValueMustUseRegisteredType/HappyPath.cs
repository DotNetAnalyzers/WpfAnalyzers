namespace WpfAnalyzers.Test.DependencyProperties.WPF0014SetValueMustUseRegisteredType
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class HappyPath : HappyPathVerifier<WPF0014SetValueMustUseRegisteredType>
    {
        [TestCase("this.SetValue(BarProperty, 1);")]
        [TestCase("this.SetCurrentValue(BarProperty, 1);")]
        public async Task DependencyProperty(string setValueCall)
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
        ""Bar"",
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public int Bar
    {
        get { return (int)GetValue(BarProperty); }
        set { SetValue(BarProperty, value); }
    }

    public void Meh()
    {
        this.SetValue(BarProperty, 1);
    }
}";
            testCode = testCode.AssertReplace("this.SetValue(BarProperty, 1);", setValueCall);
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [TestCase("this.SetValue(BarProperty, 1);")]
        [TestCase("this.SetCurrentValue(BarProperty, 1);")]
        public async Task DependencyPropertyPartial(string setValueCall)
        {
            var part1 = @"
using System.Windows;
using System.Windows.Controls;

public partial class FooControl : Control
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
        ""Bar"",
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public int Bar
    {
        get { return (int)GetValue(BarProperty); }
        set { SetValue(BarProperty, value); }
    }
}";

            var part2 = @"
public partial class FooControl
{
    public void Meh()
    {
        this.SetValue(BarProperty, 1);
    }
}";
            part2 = part2.AssertReplace("this.SetValue(BarProperty, 1);", setValueCall);
            await this.VerifyHappyPathAsync(new []{part1, part2}).ConfigureAwait(false);
        }

        [TestCase("this.SetValue(BarProperty, (object)1);")]
        [TestCase("this.SetCurrentValue(BarProperty, (object)1);")]
        public async Task DependencyPropertySetValueOfTypeObject(string setValueCall)
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
        ""Bar"",
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public int Bar
    {
        get { return (int)GetValue(BarProperty); }
        set { SetValue(BarProperty, value); }
    }

    public void Meh()
    {
        this.SetValue(BarProperty, (object)1);
    }
}";
            testCode = testCode.AssertReplace("this.SetValue(BarProperty, (object)1);", setValueCall);
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [TestCase("this.SetValue(BarProperty, value);")]
        [TestCase("this.SetCurrentValue(BarProperty, value);")]
        public async Task DependencyPropertySetValueOfTypeObject2(string setValueCall)
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
        ""Bar"",
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public int Bar
    {
        get { return (int)GetValue(BarProperty); }
        set { SetValue(BarProperty, value); }
    }

    public void Meh()
    {
        var value = this.GetValue(BarProperty);
        this.SetValue(BarProperty, value);
    }
}";
            testCode = testCode.AssertReplace("this.SetValue(BarProperty, value);", setValueCall);
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [TestCase("textBox.SetValue(TextBox.TextProperty, \"abc\");")]
        [TestCase("textBox.SetCurrentValue(TextBox.TextProperty, \"abc\");")]
        public async Task TextBoxText(string setValueCall)
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public static class Foo
{
    public static void Bar()
    {
        var textBox = new TextBox();
        textBox.SetValue(TextBox.TextProperty, ""abc"");
    }
}";
            testCode = testCode.AssertReplace("textBox.SetValue(TextBox.TextProperty, \"abc\");", setValueCall);
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task SetCurrentValueInLambda()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
        ""Bar"",
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public int Bar
    {
        get { return (int)this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }

    public void Meh()
    {
        this.Loaded += (sender, args) =>
        {
            this.SetCurrentValue(BarProperty, 1);
        };
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }
    }
}