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
            await this.VerifyHappyPathAsync(new[] { part1, part2 }).ConfigureAwait(false);
        }

        [TestCase("this.SetValue(BarProperty, 1);")]
        [TestCase("this.SetCurrentValue(BarProperty, 1);")]
        public async Task DependencyPropertyOfTypeObject(string setValueCall)
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
        ""Bar"",
        typeof(object),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public object Bar
    {
        get { return (object)GetValue(BarProperty); }
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

        [TestCase("this.SetValue(BarProperty, new Foo());")]
        [TestCase("this.SetCurrentValue(BarProperty, new Foo());")]
        public async Task DependencyPropertyOfInterfaceType(string setValueCall)
        {
            var interfaceCode = @"
public interface IFoo
{
}";

            var fooCode = @"
public class Foo : IFoo
{
}";
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
        ""Bar"",
        typeof(IFoo),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public IFoo Bar
    {
        get { return (IFoo)GetValue(BarProperty); }
        set { SetValue(BarProperty, value); }
    }

    public void Meh()
    {
        this.SetValue(BarProperty, new Foo());
    }
}";
            testCode = testCode.AssertReplace("this.SetValue(BarProperty, new Foo());", setValueCall);
            await this.VerifyHappyPathAsync(new[] { interfaceCode, fooCode, testCode }).ConfigureAwait(false);
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

        [TestCase("this.SetValue(BarProperty, true);")]
        [TestCase("this.SetCurrentValue(BarProperty, true);")]
        public async Task DependencyPropertyAddOwner(string setValueCall)
        {
            var fooCode = @"
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(bool),
            typeof(Foo),
            new PropertyMetadata(default(bool)));

        public static void SetBar(FrameworkElement element, bool value)
        {
            element.SetValue(BarProperty, value);
        }

        public static bool GetBar(FrameworkElement element)
        {
            return (bool)element.GetValue(BarProperty);
        }
    }";

            var fooControlPart1 = @"
    using System.Windows;
    using System.Windows.Controls;

    public partial class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(
            typeof(FooControl),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnVolumeChanged,
                OnVolumeCoerce));

        public bool Bar
        {
            get { return (bool)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        private static object OnVolumeCoerce(DependencyObject d, object basevalue)
        {
            return basevalue;
        }

        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }";

            var fooControlPart2 = @"
    using System.Windows;
    using System.Windows.Controls;

    public partial class FooControl
    {
        public FooControl()
        {
            this.SetValue(BarProperty, false);
        }
    }";
            fooControlPart2 = fooControlPart2.AssertReplace("this.SetValue(BarProperty, false);", setValueCall);
            await this.VerifyHappyPathAsync(new[] { fooCode, fooControlPart1, fooControlPart2 }).ConfigureAwait(false);
        }

        [TestCase("this.SetValue(VolumeProperty, 1.0);")]
        [TestCase("this.SetCurrentValue(VolumeProperty, 1.0);")]
        public async Task DependencyPropertyAddOwnerMediaElementVolume(string setValueCall)
        {
            var testCode = @"
    using System.Windows;
    using System.Windows.Controls;

    public class MediaElementWrapper : Control
    {
        public static readonly DependencyProperty VolumeProperty = MediaElement.VolumeProperty.AddOwner(
            typeof(MediaElementWrapper),
            new FrameworkPropertyMetadata(
                MediaElement.VolumeProperty.DefaultMetadata.DefaultValue,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnVolumeChanged,
                OnVolumeCoerce));

        public MediaElementWrapper()
        {
            this.SetValue(VolumeProperty, 2.0);
        }

        public double Volume
        {
            get { return (double)this.GetValue(VolumeProperty); }
            set { this.SetValue(VolumeProperty, value); }
        }

        private static object OnVolumeCoerce(DependencyObject d, object basevalue)
        {
            return basevalue;
        }

        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }";
            testCode = testCode.AssertReplace("this.SetValue(VolumeProperty, 2.0);", setValueCall);
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

        [TestCase("SetValue")]
        [TestCase("SetCurrentValue")]
        public async Task IgnoredPropertyAsParameter(string setValueCall)
        {
            var testCode = @"
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        public void Meh(DependencyProperty property, object value)
        {
            this.SetCurrentValue(property, value);
        }
    }";
            testCode = testCode.AssertReplace("SetCurrentValue", setValueCall);
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }
    }
}