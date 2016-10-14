namespace WpfAnalyzers.Test.DependencyProperties.WPF0016DefaultValueIsSharedReferenceType
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class HappyPath : HappyPathVerifier<WPF0016DefaultValueIsSharedReferenceType>
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

        [Test]
        public async Task DependencyPropertyMetadataWithCallbackOnly()
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
        new PropertyMetadata(OnValueChanged));

    public int Bar
    {
        get { return (int) this.GetValue(BarProperty); }
        set { this.SetValue(BarProperty, value); }
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // nop
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [TestCase("int", "new PropertyMetadata()")]
        [TestCase("int", "new FrameworkPropertyMetadata()")]
        [TestCase("int", "new PropertyMetadata(default(int))")]
        [TestCase("int", "new PropertyMetadata(1, OnValueChanged)")]
        [TestCase("int", "new PropertyMetadata(1)")]
        [TestCase("int?", "new PropertyMetadata(1)")]
        [TestCase("int?", "new PropertyMetadata(null)")]
        [TestCase("int?", "new PropertyMetadata(default(int?))")]
        [TestCase("Nullable<int>", "new PropertyMetadata(default(int?))")]
        [TestCase("int", "new PropertyMetadata(CreateDefaultValue())")]
        [TestCase("int", "new PropertyMetadata(CreateObjectValue())")]
        [TestCase("int[]", "new PropertyMetadata(new int[0])")]
        [TestCase("ObservableCollection<int>", "new PropertyMetadata(null)")]
        [TestCase("ObservableCollection<int>", "new PropertyMetadata(default(ObservableCollection<int>))")]
        public async Task DependencyPropertyWithMetdadata(string typeName, string metadata)
        {
            var testCode = @"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl),
        new PropertyMetadata(1));

    public double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // nop
    }

    private static double CreateDefaultValue() => default(double);
    private static object CreateObjectValue() => default(double);
}";
            testCode = testCode.AssertReplace("new PropertyMetadata(1)", metadata)
                               .AssertReplace("double", typeName);
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task DependencyPropertyWhenBoxed()
        {
            var booleanBoxesCode = @"
internal static class BooleanBoxes
{
    internal static readonly object True = true;
    internal static readonly object False = false;

    internal static object Box(bool value)
    {
        return value
                    ? True
                    : False;
    }
}";

            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(bool),
        typeof(FooControl),
        new PropertyMetadata(BooleanBoxes.Box(true)));

    public bool Value
    {
        get { return (bool)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }
}";
            await this.VerifyHappyPathAsync(new[] { testCode, booleanBoxesCode }).ConfigureAwait(false);
        }

        [Test]
        public async Task ReadOnlyDependencyProperty()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    private static readonly DependencyPropertyKey ValuePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Value),
        typeof(double),
        typeof(FooControl),
        new PropertyMetadata(1.0));

    public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

    public double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValuePropertyKey, value); }
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

    public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task AttachedPropertyWhenBoxed()
        {
            var booleanBoxesCode = @"
internal static class BooleanBoxes
{
    internal static readonly object True = true;
    internal static readonly object False = false;

    internal static object Box(bool value)
    {
        return value
                    ? True
                    : False;
    }
}";

            var testCode = @"
using System;
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
        ""Bar"",
        typeof(bool),
        typeof(Foo),
        new PropertyMetadata(BooleanBoxes.Box(true)));

    public static void SetBar(FrameworkElement element, bool value)
    {
        element.SetValue(BarProperty, value);
    }

    public static bool GetBar(FrameworkElement element)
    {
        return (bool)element.GetValue(BarProperty);
    }
}";
            await this.VerifyHappyPathAsync(new[] { testCode, booleanBoxesCode }).ConfigureAwait(false);
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
        new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

    public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }
    }
}