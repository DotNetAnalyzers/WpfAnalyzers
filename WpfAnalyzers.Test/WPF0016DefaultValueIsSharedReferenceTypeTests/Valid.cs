﻿namespace WpfAnalyzers.Test.WPF0016DefaultValueIsSharedReferenceTypeTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly PropertyMetadataAnalyzer Analyzer = new();

    [Test]
    public static void DependencyPropertyNoMetadata()
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
    public static void DependencyPropertyMetadataWithCallbackOnly()
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
    [TestCase("int?",                      "new PropertyMetadata(default(int?))")]
    [TestCase("Nullable<int>",             "new PropertyMetadata(default(int?))")]
    [TestCase("int",                       "new PropertyMetadata(CreateDefaultValue())")]
    [TestCase("int",                       "new PropertyMetadata(CreateObjectValue())")]
    [TestCase("int[]",                     "new PropertyMetadata(new int[0])")]
    [TestCase("ObservableCollection<int>", "new PropertyMetadata(null)")]
    [TestCase("ObservableCollection<int>", "new PropertyMetadata(default(ObservableCollection<int>))")]
    public static void DependencyPropertyWithMetadata(string typeName, string metadata)
    {
        var code = @"
#pragma warning disable CS8019
#nullable disable
namespace N
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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
    public static void DependencyPropertyWhenBoxed()
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
    public static void ReadOnlyDependencyProperty()
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
    public static void IgnoreFontFamily()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Media;

    public class FooControl : FrameworkElement
    {
        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
            ""FontFamily"", 
            typeof(FontFamily), 
            typeof(FooControl), 
            new PropertyMetadata(new FontFamily(""Verdana"")));

        public FontFamily FontFamily
        {
            get { return (FontFamily)this.GetValue(FontFamilyProperty); }
            set { this.SetValue(FontFamilyProperty, value); }
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void IgnoreFontFamilyAddOwner()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;

    public class FooControl : FrameworkElement
    {
        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(
            typeof(FooControl), 
            new PropertyMetadata(new FontFamily(""Verdana"")));

        public FontFamily FontFamily
        {
            get { return (FontFamily)this.GetValue(FontFamilyProperty); }
            set { this.SetValue(FontFamilyProperty, value); }
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }
}
