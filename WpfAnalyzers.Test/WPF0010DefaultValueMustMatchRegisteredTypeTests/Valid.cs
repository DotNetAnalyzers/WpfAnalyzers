namespace WpfAnalyzers.Test.WPF0010DefaultValueMustMatchRegisteredTypeTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly PropertyMetadataAnalyzer Analyzer = new();

    [Test]
    public static void DependencyPropertyRegisterNoMetadata()
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
    public static void DependencyPropertyRegisterWithMetadataWithCallbackOnly()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"", 
            typeof(int), 
            typeof(FooControl), 
            new PropertyMetadata(OnBarChanged));

        public int Bar
        {
            get { return (int) this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [TestCase("int",                       "new PropertyMetadata()")]
    [TestCase("int",                       "new FrameworkPropertyMetadata()")]
    [TestCase("int",                       "new PropertyMetadata(default(int))")]
    [TestCase("int",                       "new PropertyMetadata(1, OnValueChanged)")]
    [TestCase("int",                       "new PropertyMetadata(1)")]
    [TestCase("int?",                      "new PropertyMetadata(1)")]
    [TestCase("int?",                      "new PropertyMetadata(null)")]
    [TestCase("bool?",                     "new PropertyMetadata(null)")]
    [TestCase("bool?",                     "new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)")]
    [TestCase("int?",                      "new PropertyMetadata(default(int?))")]
    [TestCase("Nullable<int>",             "new PropertyMetadata(default(int?))")]
    [TestCase("int",                       "new PropertyMetadata(CreateDefaultValue())")]
    [TestCase("int",                       "new PropertyMetadata(CreateObjectValue())")]
    [TestCase("ObservableCollection<int>", "new PropertyMetadata(null)")]
    [TestCase("ObservableCollection<int>", "new PropertyMetadata(new ObservableCollection<int>())")]
    [TestCase("ObservableCollection<int>", "new PropertyMetadata(default(ObservableCollection<int>))")]
    [TestCase("IEnumerable",               "new PropertyMetadata(new ObservableCollection<int>())")]
    public static void DependencyPropertyRegisterWithMetadata(string typeName, string metadata)
    {
        var code = @"
#nullable disable
#pragma warning disable WPF0016, CS0105, CS8019 // Default value is shared reference type
namespace N
{
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
    }
}".AssertReplace("new PropertyMetadata(1)", metadata)
  .AssertReplace("double", typeName);

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void DependencyPropertyRegisterWhenGenericContainingType()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl<T> : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(T),
            typeof(FooControl<T>),
            new PropertyMetadata(default(T)));

        public T Bar
        {
            get { return (T)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void DependencyPropertyRegisterWhenBoxed()
    {
        var booleanBoxesCode = @"
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
    }
}";
        RoslynAssert.Valid(Analyzer, booleanBoxesCode, code);
    }

    [Test]
    public static void DependencyPropertyRegisterReadOnly()
    {
        var code = @"
namespace N
{
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
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void DependencyPropertyRegisterAttached()
    {
        var code = @"
namespace N
{
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
    }
}";

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void DependencyPropertyRegisterAttachedWhenBoxed()
    {
        var booleanBoxesCode = @"
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
            new PropertyMetadata(BooleanBoxes.Box(true)));

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
        RoslynAssert.Valid(Analyzer, booleanBoxesCode, code);
    }

    [Test]
    public static void DependencyPropertyRegisterAttachedReadOnly()
    {
        var code = @"
namespace N
{
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
    }
}";

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void DependencyPropertyAddOwner()
    {
        var fooCode = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int), 
            typeof(Foo), 
            new FrameworkPropertyMetadata(
                default(int), 
                FrameworkPropertyMetadataOptions.Inherits));

        public static void SetBar(DependencyObject element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int GetBar(DependencyObject element)
        {
            return (int) element.GetValue(BarProperty);
        }
    }
}";

        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(typeof(FooControl), new FrameworkPropertyMetadata(1));

        public int Bar
        {
            get { return (int) this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }
    }
}";

        RoslynAssert.Valid(Analyzer, code, fooCode);
    }

    [Test]
    public static void DependencyPropertyOverrideMetadata()
    {
        var fooControlCode = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            ""Value"",
            typeof(int),
            typeof(Control),
            new PropertyMetadata(default(int)));
    }
}";

        var code = @"
namespace N
{
    using System.Windows;

    public class BarControl : FooControl
    {
        static BarControl()
        {
            ValueProperty.OverrideMetadata(typeof(BarControl), new PropertyMetadata(1));
        }
    }
}";

        RoslynAssert.Valid(Analyzer, fooControlCode, code);
    }

    [Test]
    public static void CastIntToDouble()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public sealed class FooControl : Control
    {
        private const int DefaultValue = 1;

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata((double)DefaultValue));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void FontFamilyConverterConvertFromString()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Media;

    public class Foo
    {
        public static readonly DependencyProperty ButtonFontFamilyProperty = DependencyProperty.RegisterAttached(
            ""ButtonFontFamily"",
            typeof(FontFamily),
            typeof(Foo),
            new FrameworkPropertyMetadata(new FontFamilyConverter().ConvertFromString(""Marlett"")));

        /// <summary>
        /// Helper for setting ButtonFontFamily property on a DependencyObject.
        /// </summary>
        /// <param name=""element"">DependencyObject to set ButtonFontFamily property on.</param>
        /// <param name=""value"">ButtonFontFamily property value.</param>
        public static void SetButtonFontFamily(DependencyObject element, FontFamily value)
        {
            element.SetValue(ButtonFontFamilyProperty, value);
        }

        /// <summary>
        /// Helper for reading ButtonFontFamily property from a DependencyObject.
        /// </summary>
        /// <param name=""element"">DependencyObject to read ButtonFontFamily property from.</param>
        /// <returns>ButtonFontFamily property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static FontFamily GetButtonFontFamily(DependencyObject element)
        {
            return (FontFamily)element.GetValue(ButtonFontFamilyProperty);
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void BoxedBool()
    {
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
            new PropertyMetadata(BooleanBoxes.False));

        public bool IsTrue
        {
            get => (bool)GetValue(IsTrueProperty);
            set => SetValue(IsTrueProperty, BooleanBoxes.Box(value));
        }
    }

    internal static class BooleanBoxes
    {
        internal static readonly object True = true;
        internal static readonly object False = false;

        internal static object Box(bool value)
        {
            return value ? True : False;
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void EnumIssue211()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""FooEnum""/> dependency property.</summary>
        public static readonly DependencyProperty FooEnumProperty = DependencyProperty.Register(
            nameof(FooEnum),
            typeof(FooEnum),
            typeof(FooControl),
            new PropertyMetadata(FooEnum.Bar));

        public FooEnum FooEnum
        {
            get => (FooEnum) this.GetValue(FooEnumProperty);
            set => this.SetValue(FooEnumProperty, value);
        }
    }
}";
        var enumCode = @"
namespace N
{
    public enum FooEnum
    {
        Bar,
        Baz
    }
}";
        RoslynAssert.Valid(Analyzer, code,     enumCode);
        RoslynAssert.Valid(Analyzer, enumCode, code);
    }

    [Test]
    public static void EnumAddOwnerIssue211()
    {
        var fooCode = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty FooEnumProperty = DependencyProperty.RegisterAttached(
            ""FooEnum"",
            typeof(FooEnum),
            typeof(Foo),
            new FrameworkPropertyMetadata(FooEnum.Baz, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>Helper for setting <see cref=""FooEnumProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to set <see cref=""FooEnumProperty""/> on.</param>
        /// <param name=""value"">FooEnum property value.</param>
        public static void SetFooEnum(DependencyObject element, FooEnum value)
        {
            element.SetValue(FooEnumProperty, value);
        }

        /// <summary>Helper for getting <see cref=""FooEnumProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to read <see cref=""FooEnumProperty""/> from.</param>
        /// <returns>FooEnum property value.</returns>
        public static FooEnum GetFooEnum(DependencyObject element)
        {
            return (FooEnum)element.GetValue(FooEnumProperty);
        }
    }
}";
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""FooEnum""/> dependency property.</summary>
        public static readonly DependencyProperty FooEnumProperty = Foo.FooEnumProperty.AddOwner(
            typeof(FooControl),
            new FrameworkPropertyMetadata(
                FooEnum.Bar,
                FrameworkPropertyMetadataOptions.Inherits));

        public FooEnum FooEnum
        {
            get => (FooEnum) this.GetValue(FooEnumProperty);
            set => this.SetValue(FooEnumProperty, value);
        }
    }
}";
        var enumCode = @"namespace N
{
    public enum FooEnum
    {
        Bar,
        Baz
    }
}";
        RoslynAssert.Valid(Analyzer, code,     enumCode, fooCode);
        RoslynAssert.Valid(Analyzer, code,     fooCode,  enumCode);
        RoslynAssert.Valid(Analyzer, fooCode,  code,     enumCode);
        RoslynAssert.Valid(Analyzer, fooCode,  enumCode, code);
        RoslynAssert.Valid(Analyzer, enumCode, fooCode,  code);
        RoslynAssert.Valid(Analyzer, enumCode, code,     fooCode);
    }

    [Test]
    public static void SubTypeIssue354Simple()
    {
        var code = @"
namespace ValidCode.Repro
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class C : Image
    {
        public static readonly DependencyProperty IconGeometryProperty = DependencyProperty.Register(
            nameof(IconGeometry),
            typeof(Geometry),
            typeof(C),
            new PropertyMetadata(new EllipseGeometry(default, 5, 5)));

        public Geometry IconGeometry
        {
            get => (Geometry)GetValue(IconGeometryProperty);
            set => SetValue(IconGeometryProperty, value);
        }
    }
}";

        RoslynAssert.Valid(Analyzer, Descriptors.WPF0010DefaultValueMustMatchRegisteredType, code);
    }

    [Test]
    public static void SubTypeIssue354()
    {
        var typeA = @"
namespace ValidCode.Repro
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class TypeA : Image
    {
        public Geometry IconGeometry
        {
            get => (Geometry)GetValue(IconGeometryProperty);
            set => SetValue(IconGeometryProperty, value);
        }

        public static readonly DependencyProperty IconGeometryProperty =
            DependencyProperty.Register(nameof(IconGeometry), typeof(Geometry), typeof(TypeA), new PropertyMetadata(null));
    }	
}";

        var typeB = @"
namespace ValidCode.Repro
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
	
    public class TypeB : Image
    {
        public Geometry IconGeometry
        {
            get => (Geometry)GetValue(IconGeometryProperty);
            // WPF0014 SetValue must use registered type System.Windows.Media.Geometry
            set => SetValue(IconGeometryProperty, value);
        }

        public static readonly DependencyProperty IconGeometryProperty =
            // WPF0010 Default value for 'TypeA.IconGeometryProperty' must be of type System.Windows.Media.Geometry
            TypeA.IconGeometryProperty.AddOwner(typeof(TypeB), new FrameworkPropertyMetadata(new EllipseGeometry(default, 5, 5)));
    }	
}";

        RoslynAssert.Valid(Analyzer, Descriptors.WPF0010DefaultValueMustMatchRegisteredType, typeA, typeB);
    }
}