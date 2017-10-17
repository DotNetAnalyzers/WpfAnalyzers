namespace WpfAnalyzers.Test.DependencyProperties.WPF0005PropertyChangedCallbackShouldMatchRegisteredName
{
    using System.Threading.Tasks;
    using NUnit.Framework;
    using WpfAnalyzers.DependencyProperties;

    internal class HappyPath : HappyPathVerifier<WPF0005PropertyChangedCallbackShouldMatchRegisteredName>
    {
        [Test]
        public async Task DependencyPropertyNoMetadata()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl));

    public double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [TestCase("new PropertyMetadata(OnBarChanged)")]
        [TestCase("new PropertyMetadata(new PropertyChangedCallback(OnBarChanged))")]
        [TestCase("new PropertyMetadata(default(int), OnBarChanged)")]
        [TestCase("new PropertyMetadata(default(int), new PropertyChangedCallback(OnBarChanged))")]
        public async Task DependencyPropertyWithMetadata(string metadata)
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
            new PropertyMetadata(default(int), OnBarChanged));

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }";
            testCode = testCode.AssertReplace("new PropertyMetadata(default(int), OnBarChanged)", metadata);
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task ReadOnlyDependencyProperty()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    private static readonly DependencyPropertyKey ValuePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Value),
        typeof(double),
        typeof(FooControl),
        new PropertyMetadata(1.0, OnValueChanged));

    public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

    public double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValuePropertyKey, value); }
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // nop
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task AttachedProperty()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int), OnBarChanged));

    public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

    private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // nop
    }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task ReadOnlyAttachedProperty()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int), OnBarChanged));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

    private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // nop
    }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task OverrideMetadata()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : UserControl
{
    static FooControl()
    {
        BackgroundProperty.OverrideMetadata(typeof(FooControl),
            new FrameworkPropertyMetadata(null, OnBackgroundChanged));
    }

    private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // nop
    }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }
    }
}