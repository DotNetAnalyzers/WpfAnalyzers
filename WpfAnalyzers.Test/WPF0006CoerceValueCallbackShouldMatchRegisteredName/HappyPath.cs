namespace WpfAnalyzers.Test.DependencyProperties.WPF0006CoerceValueCallbackShouldMatchRegisteredName
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class HappyPath : HappyPathVerifier<WPF0006CoerceValueCallbackShouldMatchRegisteredName>
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

        [TestCase("new PropertyMetadata(null, null, CoerceBar)")]
        [TestCase("new PropertyMetadata(new CoerceValueCallback(CoerceBar))")]
        [TestCase("new PropertyMetadata(default(int), null, CoerceBar)")]
        [TestCase("new PropertyMetadata(default(int), null, new CoerceValueCallback(CoerceBar))")]
        public async Task DependencyWithPropertyMetadata(string metadata)
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
            new PropertyMetadata(default(int), null, CoerceBar));

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        private static object CoerceBar(DependencyObject d, object baseValue)
        {
            return baseValue;
        }
    }";
            testCode = testCode.AssertReplace("new PropertyMetadata(default(int), null, CoerceBar)", metadata);
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
        new PropertyMetadata(1.0, null, CoerceValue));

    public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

    public double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValuePropertyKey, value); }
    }

    private static object CoerceValue(DependencyObject d, object baseValue)
    {
        return baseValue;
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
        new PropertyMetadata(default(int), null, CoerceBar));

    public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

    private static object CoerceBar(DependencyObject d, object baseValue)
    {
        return baseValue;
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
        new PropertyMetadata(default(int), OnBarChanged, CoerceBar));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

    private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // nop
    }

    private static object CoerceBar(DependencyObject d, object baseValue)
    {
        return baseValue;
    }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }
    }
}