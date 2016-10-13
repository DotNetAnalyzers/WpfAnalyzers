namespace WpfAnalyzers.Test.DependencyProperties.WPF0004ClrMethodShouldMatchRegisteredName
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class HappyPath : HappyPathVerifier<WPF0004ClrMethodShouldMatchRegisteredName>
    {
        [TestCase("public")]
        [TestCase("private")]
        public async Task AttachedProperty(string accessModifier)
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

    public static void SetBar(FrameworkElement element, int value)
    {
        element.SetValue(BarProperty, value);
    }

    public static int GetBar(FrameworkElement element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";
            testCode = testCode.AssertReplace("    public", $"    {accessModifier}");
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [TestCase("public")]
        [TestCase("private")]
        public async Task AttachedPropertyExtensionMethods(string accessModifier)
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

    public static void SetBar(this FrameworkElement element, int value)
    {
        element.SetValue(BarProperty, value);
    }

    public static int GetBar(this FrameworkElement element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";
            testCode = testCode.AssertReplace("    public", $"    {accessModifier}");
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [TestCase("public")]
        [TestCase("private")]
        public async Task AttachedPropertyExpressionBody(string accessModifier)
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

    public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);
}";
            testCode = testCode.AssertReplace("    public", $"    {accessModifier}");
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [TestCase("public")]
        [TestCase("private")]
        public async Task ReadOnlyAttachedProperty(string accessModifier)
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);
}";
            testCode = testCode.AssertReplace("    public", $"    {accessModifier}");
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoresWhenSetterIsNotUsingValue()
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

    public static void Bar(FrameworkElement element, int value)
    {
        element.SetValue(BarProperty, 1);
    }

    public static int GetBar(FrameworkElement element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoresWhenSetterIsNotSettingElement()
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

    private static readonly FrameworkElement Element = new FrameworkElement();

    public static void Bar(FrameworkElement element, int value)
    {
        Element.SetValue(BarProperty, value);
    }

    public static int GetBar(FrameworkElement element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoresWhenGetterIsNotGettingElement()
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

    private static readonly FrameworkElement Element = new FrameworkElement();

    public static void SetBar(FrameworkElement element, int value)
    {
        element.SetValue(BarProperty, value);
    }

    public static int Meh(FrameworkElement element)
    {
        return (int)Element.GetValue(BarProperty);
    }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }
    }
}