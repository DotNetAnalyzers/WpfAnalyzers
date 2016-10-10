namespace WpfAnalyzers.Test.DependencyProperties.WPF0041SetMutableUsingSetCurrentValue
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using WpfAnalyzers.DependencyProperties;

    internal class HappyPath : HappyPathVerifier<WPF0041SetMutableUsingSetCurrentValue>
    {
        [Test]
        public async Task ReadOnlyDependencyProperty()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
        ""Bar"",
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public int Bar
    {
        get { return (int)GetValue(BarProperty); }
        protected set { SetValue(BarPropertyKey, value); }
    }
    
    public int Baz { get; set; }

    public void Meh()
    {
        Bar = 1;
        this.Bar = 2;
        this.Bar = this.CreateValue();
        Baz = 5;
        var control = new FooControl();
        control.Bar = 6;
    }

    private int CreateValue() => 4;
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task ReadOnlyDependencyPropertyFromOutside()
        {
            var fooControlCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    internal static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
        ""Bar"",
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public int Bar
    {
        get { return (int)GetValue(BarProperty); }
        internal set { SetValue(BarPropertyKey, value); }
    }
}";

            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public static class Foo
{
    public static void Meh()
    {
        var fooControl = new FooControl();
        fooControl.Bar = 1;
        fooControl.SetValue(FooControl.BarPropertyKey, 1);
        fooControl.Bar = CreateValue();
        fooControl.SetValue(FooControl.BarPropertyKey, CreateValue());
        fooControl.SetValue(FooControl.BarPropertyKey, CreateObjectValue());
    }

    private static int CreateValue() => 4;
    private static object CreateObjectValue() => 4;
}";
            await this.VerifyHappyPathAsync(new[] { testCode, fooControlCode }).ConfigureAwait(false);
        }

        [Test]
        public async Task ReadOnlyDependencyPropertyThis()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
        ""Bar"",
        typeof(int),
        typeof(FooControl),
        new PropertyMetadata(default(int)));

    public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

    public int Bar
    {
        get { return (int)this.GetValue(BarProperty); }
        set { this.SetValue(BarPropertyKey, value); }
    }
    
    public int Baz { get; set; }

    public void Meh()
    {
        Bar = 1;
        this.Bar = 2;
        this.Bar = this.CreateValue();
        Baz = 5;
        var control = new FooControl();
        control.Bar = 6;
    }

    private int CreateValue() => 4;
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task AttachedProperty()
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
            await this.VerifyHappyPathAsync(new[] { testCode, booleanBoxesCode }).ConfigureAwait(false);
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
        new PropertyMetadata(default(bool)));

    public static void SetBar(FrameworkElement element, bool value)
    {
        element.SetValue(BarProperty, BooleanBoxes.Box(value));
    }

    public static bool GetBar(FrameworkElement element)
    {
        return (bool)element.GetValue(BarProperty);
    }
}";
            await this.VerifyHappyPathAsync(new[] { testCode, booleanBoxesCode }).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoredDependencyPropertyInClrProperty()
        {
            var testCode = @"
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
        public async Task IgnoredDependencyPropertyInClrPropertyBoxed()
        {
            var boolBoxesCode = @"

public static class BooleanBoxes
{
    public static readonly object True = true;
    public static readonly object False = false;
}";

            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty IsTrueProperty = DependencyProperty.Register(
        nameof(IsTrue),
        typeof(bool),
        typeof(FooControl),
        new PropertyMetadata(default(bool)));

    public bool IsTrue
    {
        get { return (bool)this.GetValue(IsTrueProperty); }
        set { this.SetValue(IsTrueProperty, value ? BooleanBoxes.True : BooleanBoxes.False); }
    }
}";
            await this.VerifyHappyPathAsync(new []{testCode, boolBoxesCode}).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoredAttachedPropertyInClrSetMethod()
        {
            var testCode = @"
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty IsTrueProperty = DependencyProperty.RegisterAttached(
        ""IsTrue"",
        typeof(bool),
        typeof(Foo),
        new PropertyMetadata(default(bool)));

    public static void SetIsTrue(this DependencyObject element, bool value)
    {
        element.SetValue(IsTrueProperty, value);
    }

    [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
    [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
    public static bool GetIsTrue(this DependencyObject element)
    {
        return (bool)element.GetValue(IsTrueProperty);
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoredAttachedPropertyInClrSetMethodWhenBoxed()
        {
            var boolBoxesCode = @"

public static class BooleanBoxes
{
    public static readonly object True = true;
    public static readonly object False = false;
}";

            var testCode = @"
using System.Windows;

public static class Foo
{
    public static readonly DependencyProperty IsTrueProperty = DependencyProperty.RegisterAttached(
        ""IsTrue"",
        typeof(bool),
        typeof(Foo),
        new PropertyMetadata(default(bool)));

    public static void SetIsTrue(this DependencyObject element, bool value)
    {
        element.SetValue(IsTrueProperty, value ? BooleanBoxes.True : BooleanBoxes.False);
    }

    [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
    [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
    public static bool GetIsTrue(this DependencyObject element)
    {
        return (bool)element.GetValue(IsTrueProperty);
    }
}";
            await this.VerifyHappyPathAsync(new[] { testCode, boolBoxesCode }).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoredClrPropertyInObjectInitializer()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public static class Foo
{
    public static void Bar()
    {
        var textBlock = new TextBlock
        {
            Text = ""abc"",
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = false
        };
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoredClrPropertyInConstructor()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl));

    public FooControl()
    {
        this.Value = 2;
    }

    public double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task IgnoredSetValueInConstructor()
        {
            var testCode = @"
using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(FooControl));

    public FooControl()
    {
        SetValue(ValueProperty, 2);
        this.SetValue(ValueProperty, 2);
    }

    public double Value
    {
        get { return (double)this.GetValue(ValueProperty); }
        set { this.SetValue(ValueProperty, value); }
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }
    }
}