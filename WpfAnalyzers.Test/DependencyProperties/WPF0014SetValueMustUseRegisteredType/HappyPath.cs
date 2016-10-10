namespace WpfAnalyzers.Test.DependencyProperties.WPF0014SetValueMustUseRegisteredType
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class HappyPath : HappyPathVerifier<WPF0014SetValueMustUseRegisteredType>
    {
        [Test]
        public async Task DependencyProperty()
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
        this.SetCurrentValue(BarProperty, 1);
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertySetValueOfTypeObject()
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
        this.SetCurrentValue(BarProperty, (object)1);
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertySetValueOfTypeObject2()
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
        this.SetCurrentValue(BarProperty, value);
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task TextBoxText()
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
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

    }
}