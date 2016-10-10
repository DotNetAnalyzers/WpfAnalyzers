namespace WpfAnalyzers.Test.DependencyProperties.WPF0001BackingFieldShouldMatchRegisteredName
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class HappyPath : HappyPathVerifier<WPF0001BackingFieldShouldMatchRegisteredName>
    {
        [TestCase("\"Bar\"")]
        [TestCase("nameof(Bar)")]
        [TestCase("nameof(FooControl.Bar)")]
        public async Task DependencyPropertyOneLine(string nameof)
        {
            var testCode = @"
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(nameof(Bar), typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }";
            testCode = testCode.AssertReplace("nameof(Bar)", nameof);
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertyFormatted()
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
        new PropertyMetadata(default(int)));

    public static void SetBar(DependencyObject element, int value)
    {
        element.SetValue(BarProperty, value);
    }

    public static int GetBar(DependencyObject element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoresReadonlyDependencyProperty()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    private static readonly DependencyPropertyKey ErrorPropertyKey = DependencyProperty.RegisterReadOnly(
        ""Bar"",
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty BarProperty = ErrorPropertyKey.DependencyProperty;

    public int Bar
    {
        get { return (int)GetValue(BarProperty); }
        set { SetValue(ErrorPropertyKey, value); }
    }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoresReadOnlyAttachedProperty()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    private static readonly DependencyPropertyKey ErrorKey = DependencyProperty.RegisterAttachedReadOnly(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty BarProperty = ErrorKey.DependencyProperty;

    public static void SetBar(DependencyObject element, int value)
    {
        element.SetValue(ErrorKey, value);
    }

    public static int GetBar(DependencyObject element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }
    }
}