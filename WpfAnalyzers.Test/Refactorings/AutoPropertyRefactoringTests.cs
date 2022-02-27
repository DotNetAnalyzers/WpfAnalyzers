namespace WpfAnalyzers.Test.Refactorings;

using Gu.Roslyn.Asserts;

using NUnit.Framework;

using WpfAnalyzers.Refactorings;

public static class AutoPropertyRefactoringTests
{
    private static readonly AutoPropertyRefactoring Refactoring = new();

    [Test]
    public static void AutoProperty()
    {
        var before = @"
namespace N
{
    using System.Windows.Controls;

    public class C : Control
    {
        public int ↓Number { get; set; }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class C : Control
    {
        /// <summary>Identifies the <see cref=""Number""/> dependency property.</summary>
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            nameof(Number),
            typeof(int),
            typeof(C),
            new PropertyMetadata(default(int)));

        public int Number
        {
            get => (int)this.GetValue(NumberProperty);
            set => this.SetValue(NumberProperty, value);
        }
    }
}";
        RoslynAssert.Refactoring(Refactoring, before, after);
    }

    [Test]
    public static void AutoProperty2()
    {
        var before = @"
namespace N
{
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public double? ↓Value { get; set; }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double?),
            typeof(FooControl),
            new PropertyMetadata(default(double?)));

        public double? Value
        {
            get => (double?)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }
    }
}";
        RoslynAssert.Refactoring(Refactoring, before, after);
    }

    [Test]
    public static void AutoPropertyNotQualifiedMethodAccess()
    {
        var before = @"
namespace N
{
    using System.Windows.Controls;

    public class C : Control
    {
        public int ↓Number { get; set; }

        public int M() => M();
    }
}";

        var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class C : Control
    {
        /// <summary>Identifies the <see cref=""Number""/> dependency property.</summary>
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            nameof(Number),
            typeof(int),
            typeof(C),
            new PropertyMetadata(default(int)));

        public int Number
        {
            get => (int)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }

        public int M() => M();
    }
}";
        RoslynAssert.Refactoring(Refactoring, before, after);
    }

    [Test]
    public static void InsertsBackingFieldAtCorrectPosition()
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class C : Control
    {
        /// <summary>Identifies the <see cref=""Value1""/> dependency property.</summary>
        public static readonly DependencyProperty Value1Property = DependencyProperty.Register(
            nameof(Value1),
            typeof(int),
            typeof(C),
            new PropertyMetadata(default(int)));

        /// <summary>Identifies the <see cref=""Value3""/> dependency property.</summary>
        public static readonly DependencyProperty Value3Property = DependencyProperty.Register(
            nameof(Value3),
            typeof(int),
            typeof(C),
            new PropertyMetadata(default(int)));

        public int Value1
        {
            get => (int)this.GetValue(Value1Property);
            set => this.SetValue(Value1Property, value);
        }

        public int ↓Value2 { get; set; }

        public int Value3
        {
            get => (int)this.GetValue(Value3Property);
            set => this.SetValue(Value3Property, value);
        }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class C : Control
    {
        /// <summary>Identifies the <see cref=""Value1""/> dependency property.</summary>
        public static readonly DependencyProperty Value1Property = DependencyProperty.Register(
            nameof(Value1),
            typeof(int),
            typeof(C),
            new PropertyMetadata(default(int)));

        /// <summary>Identifies the <see cref=""Value2""/> dependency property.</summary>
        public static readonly DependencyProperty Value2Property = DependencyProperty.Register(
            nameof(Value2),
            typeof(int),
            typeof(C),
            new PropertyMetadata(default(int)));

        /// <summary>Identifies the <see cref=""Value3""/> dependency property.</summary>
        public static readonly DependencyProperty Value3Property = DependencyProperty.Register(
            nameof(Value3),
            typeof(int),
            typeof(C),
            new PropertyMetadata(default(int)));

        public int Value1
        {
            get => (int)this.GetValue(Value1Property);
            set => this.SetValue(Value1Property, value);
        }

        public int Value2
        {
            get => (int)this.GetValue(Value2Property);
            set => this.SetValue(Value2Property, value);
        }

        public int Value3
        {
            get => (int)this.GetValue(Value3Property);
            set => this.SetValue(Value3Property, value);
        }
    }
}";
        RoslynAssert.Refactoring(Refactoring, before, after, title: "Change to dependency property");
    }

    [Test]
    public static void InsertsReadonlyBackingFieldsAtCorrectPosition()
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class C : Control
    {
        private static readonly DependencyPropertyKey Value1PropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Value1),
            typeof(int),
            typeof(C),
            new PropertyMetadata(default(int)));

        /// <summary>Identifies the <see cref=""Value1""/> dependency property.</summary>
        public static readonly DependencyProperty Value1Property = Value1PropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey Value3PropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Value3),
            typeof(int),
            typeof(C),
            new PropertyMetadata(default(int)));

        /// <summary>Identifies the <see cref=""Value3""/> dependency property.</summary>
        public static readonly DependencyProperty Value3Property = Value3PropertyKey.DependencyProperty;

        public int Value1
        {
            get => (int)this.GetValue(Value1Property);
            private set => this.SetValue(Value1PropertyKey, value);
        }

        public int ↓Value2 { get; private set; }

        public int Value3
        {
            get => (int)this.GetValue(Value3Property);
            private set => this.SetValue(Value3PropertyKey, value);
        }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class C : Control
    {
        private static readonly DependencyPropertyKey Value1PropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Value1),
            typeof(int),
            typeof(C),
            new PropertyMetadata(default(int)));

        /// <summary>Identifies the <see cref=""Value1""/> dependency property.</summary>
        public static readonly DependencyProperty Value1Property = Value1PropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey Value2PropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Value2),
            typeof(int),
            typeof(C),
            new PropertyMetadata(default(int)));

        /// <summary>Identifies the <see cref=""Value2""/> dependency property.</summary>
        public static readonly DependencyProperty Value2Property = Value2PropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey Value3PropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Value3),
            typeof(int),
            typeof(C),
            new PropertyMetadata(default(int)));

        /// <summary>Identifies the <see cref=""Value3""/> dependency property.</summary>
        public static readonly DependencyProperty Value3Property = Value3PropertyKey.DependencyProperty;

        public int Value1
        {
            get => (int)this.GetValue(Value1Property);
            private set => this.SetValue(Value1PropertyKey, value);
        }

        public int Value2
        {
            get => (int)this.GetValue(Value2Property);
            private set => this.SetValue(Value2PropertyKey, value);
        }

        public int Value3
        {
            get => (int)this.GetValue(Value3Property);
            private set => this.SetValue(Value3PropertyKey, value);
        }
    }
}";
        RoslynAssert.Refactoring(Refactoring, before, after, title: "Change to readonly dependency property");
    }

    [Test]
    public static void StaticAutoProperty()
    {
        var before = @"
namespace N
{
    public static class C
    {
        public static int ↓Number { get; set; }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;

    public static class C
    {
        public static readonly DependencyProperty NumberProperty = DependencyProperty.RegisterAttached(
            ""Number"",
            typeof(int),
            typeof(C),
            new PropertyMetadata(default(int)));

        /// <summary>Helper for getting <see cref=""NumberProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to read <see cref=""NumberProperty""/> from.</param>
        /// <returns>Number property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static int GetNumber(DependencyObject element)
        {
            return (int)element.GetValue(NumberProperty);
        }

        /// <summary>Helper for setting <see cref=""NumberProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to set <see cref=""NumberProperty""/> on.</param>
        /// <param name=""value"">Number property value.</param>
        public static void SetNumber(DependencyObject element, int value)
        {
            element.SetValue(NumberProperty, value);
        }
    }
}";
        RoslynAssert.Refactoring(Refactoring, before, after);
    }

    [Test]
    public static void StaticAutoProperty2()
    {
        var before = @"
namespace N
{
    public static class Foo
    {
        public static double ↓Value { get; set; }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
            ""Value"",
            typeof(double),
            typeof(Foo),
            new PropertyMetadata(default(double)));

        /// <summary>Helper for getting <see cref=""ValueProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to read <see cref=""ValueProperty""/> from.</param>
        /// <returns>Value property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static double GetValue(DependencyObject element)
        {
            return (double)element.GetValue(ValueProperty);
        }

        /// <summary>Helper for setting <see cref=""ValueProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to set <see cref=""ValueProperty""/> on.</param>
        /// <param name=""value"">Value property value.</param>
        public static void SetValue(DependencyObject element, double value)
        {
            element.SetValue(ValueProperty, value);
        }
    }
}";
        RoslynAssert.Refactoring(Refactoring, before, after);
    }
}