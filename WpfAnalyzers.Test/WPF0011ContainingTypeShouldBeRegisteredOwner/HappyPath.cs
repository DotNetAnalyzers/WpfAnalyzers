namespace WpfAnalyzers.Test.WPF0011ContainingTypeShouldBeRegisteredOwner
{
    using System.Threading.Tasks;
    using NUnit.Framework;
    using WPF0011ContainingTypeShouldBeRegisteredOwner = WpfAnalyzers.WPF0011ContainingTypeShouldBeRegisteredOwner;

    internal class HappyPath : HappyPathVerifier<WPF0011ContainingTypeShouldBeRegisteredOwner>
    {
        [TestCase("FooControl")]
        [TestCase("FooControl<T>")]
        public async Task DependencyPropertyRegister(string typeName)
        {
            var testCode = @"
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    // registering for an owner that is not containing type.
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
}";
            testCode = testCode.AssertReplace("FooControl", typeName);
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertyRegisterReadOnly()
        {
            var testCode = @"
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Bar),
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public int Bar
    {
        get { return (int)this.GetValue(BarProperty); }
        private set { this.SetValue(BarPropertyKey, value); }
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertyRegisterAttached()
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

    public static void SetBar(this DependencyObject element, int value)
    {
        element.SetValue(BarProperty, value);
    }

    [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
    [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
    public static int GetBar(this DependencyObject element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertyRegisterAttachedReadOnly()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    public static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        ""Bar"",
        typeof(int),
        typeof(Foo),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public static void SetBar(this DependencyObject element, int value)
    {
        element.SetValue(BarPropertyKey, value);
    }

    [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
    [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
    public static int GetBar(this DependencyObject element)
    {
        return (int)element.GetValue(BarProperty);
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [TestCase("FooControl")]
        [TestCase("FooControl<T>")]
        public async Task DependencyPropertyAddOwner(string typeName)
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

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

    [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
    [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
    public static int GetBar(DependencyObject element)
    {
        return (int)element.GetValue(BarProperty);
    }
}

public class FooControl : Control
{
    public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(typeof(FooControl));

    public double Bar
    {
        get { return (double)this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }
}";
            testCode = testCode.AssertReplace("FooControl", typeName);
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertyOverrideMetadata()
        {
            var fooControlCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public int Value
    {
        get { return (int)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }
}";

            var barControlCode = @"
using System.Windows;
using System.Windows.Controls;

public class BarControl : FooControl
{
    static BarControl()
    {
        ValueProperty.OverrideMetadata(typeof(BarControl), new PropertyMetadata(1));
    }
}";

            await this.VerifyHappyPathAsync(new[] { fooControlCode, barControlCode }).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoreDependencyPropertyOverrideMetadataWhenContainingTypeIsNotSubclassOfOwningType()
        {
            var testCode = @"
namespace Meya
{
    using System.Globalization;
    using System.Windows;
    using System.Windows.Markup;

    public partial class App : Application
    {
        static App()
        {
            // Ensure that we are using the right culture
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }
    }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }
    }
}