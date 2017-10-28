namespace WpfAnalyzers.Test.WPF0041SetMutableUsingSetCurrentValueTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class HappyPath
    {
        private static readonly WPF0041SetMutableUsingSetCurrentValue Analyzer = new WPF0041SetMutableUsingSetCurrentValue();

        [Test]
        public void DependencyProperty()
        {
            var testCode = @"
namespace RoslynSandbox
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

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("this.fooControl.SetCurrentValue(FooControl.BarProperty, 1);")]
        [TestCase("this.fooControl?.SetCurrentValue(FooControl.BarProperty, 1);")]
        public void DependencyPropertyFromOutside(string setExpression)
        {
            var fooCode = @"
namespace RoslynSandbox
{
    public class Foo
    {
        private readonly FooControl fooControl = new FooControl();

        public void Meh()
        {
            this.fooControl.SetCurrentValue(FooControl.BarProperty, 1);
        }
    }";
            var fooControlCode = @"
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
            fooCode = fooCode.AssertReplace(
                "this.fooControl.SetCurrentValue(FooControl.BarProperty, 1);",
                setExpression);
            AnalyzerAssert.Valid(Analyzer, fooCode, fooControlCode);
        }

        [Test]
        public void ReadOnlyDependencyProperty()
        {
            var testCode = @"
namespace RoslynSandbox
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void ReadOnlyDependencyPropertyFromOutside()
        {
            var fooControlCode = @"
namespace RoslynSandbox
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

            var testCode = @"
namespace RoslynSandbox
{
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
    }
}";
            AnalyzerAssert.Valid(Analyzer, fooControlCode, testCode);
        }

        [Test]
        public void ReadOnlyDependencyPropertyThis()
        {
            var testCode = @"
namespace RoslynSandbox
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void DependencyPropertyRegisterAttached()
        {
            var booleanBoxesCode = @"
namespace RoslynSandbox
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

            var testCode = @"
namespace RoslynSandbox
{
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
    }
}";
            AnalyzerAssert.Valid(Analyzer, booleanBoxesCode, testCode);
        }

        [Test]
        public void DependencyPropertyRegisterAttachedWhenBoxed()
        {
            var booleanBoxesCode = @"
namespace RoslynSandbox
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

            var testCode = @"
namespace RoslynSandbox
{
    using System;
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
            AnalyzerAssert.Valid(Analyzer, booleanBoxesCode, testCode);
        }

        [Test]
        public void IgnoredDependencyPropertyInClrProperty()
        {
            var testCode = @"
namespace RoslynSandbox
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IgnoredDependencyPropertyInClrPropertyWithAsCast()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(string),
            typeof(FooControl));

        public string Value
        {
            get { return this.GetValue(ValueProperty) as string; }
            set { this.SetValue(ValueProperty, value); }
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IgnoredDependencyPropertyInClrPropertyBoxed()
        {
            var boolBoxesCode = @"
namespace RoslynSandbox
{
    public static class BooleanBoxes
    {
        public static readonly object True = true;
        public static readonly object False = false;
    }
}";

            var testCode = @"
namespace RoslynSandbox
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
            AnalyzerAssert.Valid(Analyzer, boolBoxesCode, testCode);
        }

        [Test]
        public void IgnoredAttachedPropertyInClrSetMethod()
        {
            var testCode = @"
namespace RoslynSandbox
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IgnoredAttachedPropertyInClrSetMethodWhenBoxed()
        {
            var boolBoxesCode = @"
namespace RoslynSandbox
{
    public static class BooleanBoxes
    {
        public static readonly object True = true;
        public static readonly object False = false;
    }
}";

            var testCode = @"
namespace RoslynSandbox
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
            AnalyzerAssert.Valid(Analyzer, boolBoxesCode, testCode);
        }

        [Test]
        public void IgnoredClrPropertyInObjectInitializer()
        {
            var testCode = @"
namespace RoslynSandbox
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IgnoredClrPropertyInConstructor()
        {
            var testCode = @"
namespace RoslynSandbox
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IgnoredSetValueInConstructor()
        {
            var testCode = @"
namespace RoslynSandbox
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("textBox.Visibility = Visibility.Hidden;")]
        [TestCase("textBox.SetValue(TextBox.VisibilityProperty, Visibility.Hidden);")]
        public void IgnoredWhenCreatedInScope(string setCall)
        {
            var testCode = @"
namespace RoslynSandbox
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
}";
            testCode = testCode.AssertReplace("textBox.Visibility = Visibility.Hidden;", setCall);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("textBox.Visibility = Visibility.Hidden;")]
        [TestCase("textBox.SetValue(TextBox.VisibilityProperty, Visibility.Hidden);")]
        public void IgnoredWhenCreatedInScopeWithBeginEndInit(string setCall)
        {
            var testCode = @"
namespace RoslynSandbox
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
}";
            testCode = testCode.AssertReplace("textBox.Visibility = Visibility.Hidden;", setCall);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("textBox.Visibility = Visibility.Hidden;")]
        [TestCase("textBox.SetValue(TextBox.VisibilityProperty, Visibility.Hidden);")]
        public void IgnoredWhenCreatedInScopeWithIf(string setCall)
        {
            var testCode = @"
namespace RoslynSandbox
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
}";
            testCode = testCode.AssertReplace("textBox.Visibility = Visibility.Hidden;", setCall);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("SetValue")]
        [TestCase("SetCurrentValue")]
        public void IgnoredPropertyAsParameter(string setValueCall)
        {
            var testCode = @"
namespace RoslynSandbox
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
}";
            testCode = testCode.AssertReplace("SetCurrentValue", setValueCall);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IgnoreSetDataContext()
        {
            var testCode = @"
namespace RoslynSandbox
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

        public static void Meh()
        {
            var control = new Control();
            control.SetValue(FrameworkElement.DataContextProperty, 1);
            control.DataContext = 1;
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}