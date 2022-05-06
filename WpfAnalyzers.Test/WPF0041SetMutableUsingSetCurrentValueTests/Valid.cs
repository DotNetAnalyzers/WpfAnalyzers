namespace WpfAnalyzers.Test.WPF0041SetMutableUsingSetCurrentValueTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly WPF0041SetMutableUsingSetCurrentValue Analyzer = new();

    [Test]
    public static void DependencyProperty()
    {
        var code = @"
namespace N
{
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

        public void Meh()
        {
            this.SetCurrentValue(BarProperty, 1);
        }
    }
}";

        RoslynAssert.Valid(Analyzer, code);
    }

    [TestCase("this.fooControl.SetCurrentValue(FooControl.BarProperty, 1);")]
    [TestCase("this.fooControl?.SetCurrentValue(FooControl.BarProperty, 1);")]
    public static void DependencyPropertyFromOutside(string setExpression)
    {
        var fooCode = @"
namespace N
{
    public class Foo
    {
        private readonly FooControl fooControl = new FooControl();

        public void Meh()
        {
            this.fooControl.SetCurrentValue(FooControl.BarProperty, 1);
        }
    }
}".AssertReplace("this.fooControl.SetCurrentValue(FooControl.BarProperty, 1);", setExpression);
        var fooControlCode = @"
namespace N
{
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

        public void Meh()
        {
            this.SetCurrentValue(BarProperty, 1);
        }
    }
}";
        RoslynAssert.Valid(Analyzer, fooCode, fooControlCode);
    }

    [Test]
    public static void ReadOnlyDependencyProperty()
    {
        var code = @"
namespace N
{
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
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void ReadOnlyDependencyPropertyFromOutside()
    {
        var fooControlCode = @"
namespace N
{
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
    }
}";

        var code = @"
namespace N
{
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
    }
}";
        RoslynAssert.Valid(Analyzer, fooControlCode, code);
    }

    [Test]
    public static void ReadOnlyDependencyPropertyThis()
    {
        var code = @"
namespace N
{
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
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void DependencyPropertyRegisterAttached()
    {
        var booleanBoxes = @"
namespace N
{
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
    }
}";

        var code = @"
namespace N
{
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
    }
}";
        RoslynAssert.Valid(Analyzer, booleanBoxes, code);
    }

    [Test]
    public static void DependencyPropertyRegisterAttachedWhenBoxed()
    {
        var booleanBoxes = @"
namespace N
{
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
    }
}";

        var code = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(bool),
            typeof(Foo),
            new PropertyMetadata(BooleanBoxes.False));

        public static void SetBar(FrameworkElement element, bool value)
        {
            element.SetValue(BarProperty, BooleanBoxes.Box(value));
        }

        public static bool GetBar(FrameworkElement element)
        {
            return (bool)element.GetValue(BarProperty);
        }
    }
}";
        RoslynAssert.Valid(Analyzer, booleanBoxes, code);
    }

    [Test]
    public static void IgnoredDependencyPropertyInClrProperty()
    {
        var code = @"
namespace N
{
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
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void IgnoredDependencyPropertyInClrPropertyWithAsCast()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(string),
            typeof(FooControl));

        public string? Value
        {
            get { return this.GetValue(ValueProperty) as string; }
            set { this.SetValue(ValueProperty, value); }
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void IgnoredDependencyPropertyInClrPropertyBoxed()
    {
        var boolBoxesCode = @"
namespace N
{
    public static class BooleanBoxes
    {
        public static readonly object True = true;
        public static readonly object False = false;
    }
}";

        var code = @"
namespace N
{
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
    }
}";
        RoslynAssert.Valid(Analyzer, boolBoxesCode, code);
    }

    [Test]
    public static void IgnoredAttachedPropertyInClrSetMethod()
    {
        var code = @"
namespace N
{
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
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void IgnoredAttachedPropertyInClrSetMethodWhenBoxedTernary()
    {
        var boolBoxesCode = @"
namespace N
{
    public static class BooleanBoxes
    {
        public static readonly object True = true;
        public static readonly object False = false;
    }
}";

        var code = @"
namespace N
{
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
    }
}";
        RoslynAssert.Valid(Analyzer, boolBoxesCode, code);
    }

    [Test]
    public static void IgnoredClrPropertyInObjectInitializer()
    {
        var code = @"
namespace N
{
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
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void IgnoredClrPropertyInConstructor()
    {
        var code = @"
namespace N
{
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
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void IgnoredSetValueInConstructor()
    {
        var code = @"
namespace N
{
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
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [TestCase("textBox.Visibility = Visibility.Hidden;")]
    [TestCase("textBox.SetValue(TextBox.VisibilityProperty, Visibility.Hidden);")]
    public static void IgnoredWhenCreatedInScope(string setCall)
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public static class Foo
    {
        public static void MethodName()
        {
            var textBox = new TextBox();
            textBox.Visibility = Visibility.Hidden;
        }
    }
}".AssertReplace("textBox.Visibility = Visibility.Hidden;", setCall);

        RoslynAssert.Valid(Analyzer, code);
    }

    [TestCase("textBox.Visibility = Visibility.Hidden;")]
    [TestCase("textBox.SetValue(TextBox.VisibilityProperty, Visibility.Hidden);")]
    public static void IgnoredWhenCreatedInScopeWithBeginEndInit(string setCall)
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public static class Foo
    {
        public static void MethodName()
        {
            var textBox = new TextBox();
            textBox.BeginInit();
            textBox.Visibility = Visibility.Hidden;
            textBox.EndInit();
        }
    }
}".AssertReplace("textBox.Visibility = Visibility.Hidden;", setCall);

        RoslynAssert.Valid(Analyzer, code);
    }

    [TestCase("textBox.Visibility = Visibility.Hidden;")]
    [TestCase("textBox.SetValue(TextBox.VisibilityProperty, Visibility.Hidden);")]
    public static void IgnoredWhenCreatedInScopeWithIf(string setCall)
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public static class Foo
    {
        public static void MethodName()
        {
            var textBox = new TextBox();
            if (true)
            {
                textBox.Visibility = Visibility.Hidden;
            }
        }
    }
}".AssertReplace("textBox.Visibility = Visibility.Hidden;", setCall);

        RoslynAssert.Valid(Analyzer, code);
    }

    [TestCase("SetValue")]
    [TestCase("SetCurrentValue")]
    public static void IgnoredPropertyAsParameter(string setValueCall)
    {
        var code = @"
namespace N
{
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
    }
}".AssertReplace("SetCurrentValue", setValueCall);

        RoslynAssert.Valid(Analyzer, code);
    }

    [TestCase("control.DataContext = 1")]
    [TestCase("control.SetValue(FrameworkElement.DataContextProperty, 1)")]
    [TestCase("control.Style = new Style(typeof(FooControl))")]
    [TestCase("control.SetValue(FrameworkElement.StyleProperty, new Style(typeof(FooControl)))")]
    public static void IgnoreProperties(string expression)
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public FooControl()
        {
            this.DataContext = 1;
            DataContext = 1;
        }

        public static void Meh(DependencyProperty notUsed)
        {
            var control = new Control();
            control.DataContext = 1;
        }
    }
}".AssertReplace("control.DataContext = 1", expression);

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void Issue240()
    {
        var code = @"
namespace N
{
    public class Foo
    {
        public Foo()
        {
            this.Data2D = new int[3, 3];
            for (var i = 0; i < 3; ++i)
            {
                for (var j = 0; j < 3; ++j)
                {
                    this.Data2D[i, j] = i * j;
                }
            }
        }

        public int[,] Data2D { get; set; }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void BooleanBoxes()
    {
        var boxes = @"
namespace N
{
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
    }
}
";
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class UsingBoolBoxes : Control
    {
        /// <summary>Identifies the <see cref=""IsTrue""/> dependency property.</summary>
        public static readonly DependencyProperty IsTrueProperty = DependencyProperty.Register(
            nameof(IsTrue),
            typeof(bool),
            typeof(UsingBoolBoxes),
            new PropertyMetadata(BooleanBoxes.False));

        public bool IsTrue
        {
            get => Equals(BooleanBoxes.True, this.GetValue(IsTrueProperty));
            set => this.SetValue(IsTrueProperty, BooleanBoxes.Box(value));
        }
    }
}";

        RoslynAssert.Valid(Analyzer, boxes, code);
    }

    [Test]
    public static void WhenPrivate()
    {
        var code = @"
namespace ValidCode.Issues;

using System.Windows;

public class Issue376 : FrameworkElement
{
    private static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(Issue376),
        new PropertyMetadata(default(string)));

    private string? Text
    {
        get => (string?)this.GetValue(TextProperty);
        set => this.SetValue(TextProperty, value);
    }

    public void M(string text)
    {
        this.Text = text;
    }
}
";

        RoslynAssert.Valid(Analyzer, code);
    }
}
