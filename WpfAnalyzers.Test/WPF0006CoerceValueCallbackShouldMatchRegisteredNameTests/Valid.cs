﻿namespace WpfAnalyzers.Test.WPF0006CoerceValueCallbackShouldMatchRegisteredNameTests;

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

    [TestCase("new PropertyMetadata(OnBarChanged)")]
    [TestCase("new PropertyMetadata(new PropertyChangedCallback(OnBarChanged))")]
    [TestCase("new PropertyMetadata(default(int), OnBarChanged)")]
    [TestCase("new PropertyMetadata(default(int), new PropertyChangedCallback(OnBarChanged))")]
    [TestCase("new PropertyMetadata((o, e) => { })")]
    [TestCase("new FrameworkPropertyMetadata((o, e) => { })")]
    [TestCase("new FrameworkPropertyMetadata(OnBarChanged)")]
    [TestCase("new FrameworkPropertyMetadata(OnBarChanged, CoerceBar)")]
    [TestCase("new PropertyMetadata(default(int), null, CoerceBar)")]
    [TestCase("new PropertyMetadata(default(int), null, new CoerceValueCallback(CoerceBar))")]
    public static void DependencyPropertyWithMetadata(string metadata)
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
            new PropertyMetadata(default(int), null, CoerceBar));

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }

        private static object? CoerceBar(DependencyObject d, object? baseValue)
        {
            if (baseValue is int i &&
                i < 0)
            {
                return 0;
            }

            return baseValue;
        }
    }
}".AssertReplace("new PropertyMetadata(default(int), null, CoerceBar)", metadata);

        RoslynAssert.Valid(Analyzer, code);
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
            new PropertyMetadata(1.0, null, CoerceValue));

        public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

        public double Value
        {
            get { return (double)this.GetValue(ValueProperty); }
            set { this.SetValue(ValuePropertyKey, value); }
        }

        private static object? CoerceValue(DependencyObject d, object? baseValue)
        {
            if (baseValue is int i &&
                i < 0)
            {
                return 0;
            }

            return baseValue;
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
            new PropertyMetadata(default(int), null, CoerceBar));

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

        private static object? CoerceBar(DependencyObject d, object? baseValue)
        {
            if (baseValue is int i &&
                i < 0)
            {
                return 0;
            }

            return baseValue;
        }
    }
}";

        RoslynAssert.Valid(Analyzer, code);
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
            new PropertyMetadata(default(int), OnBarChanged, CoerceBar));

            public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }

        private static object? CoerceBar(DependencyObject d, object? baseValue)
        {
            if (baseValue is int i &&
                i < 0)
            {
                return 0;
            }

            return baseValue;
        }
    }
}";

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void Min()
    {
        var code = @"
#pragma warning disable WPF0023
namespace N;

using System;
using System.Windows;

public class Chart : FrameworkElement
{
    /// <summary>Identifies the <see cref=""Time""/> dependency property.</summary>
    public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
        nameof(Time),
        typeof(DateTimeOffset),
        typeof(Chart),
        new FrameworkPropertyMetadata(
            default(DateTimeOffset),
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            propertyChangedCallback: null,
            coerceValueCallback: (_, o) => Min(DateTimeOffset.Now, (DateTimeOffset)o)));

    public DateTimeOffset Time
    {
        get => (DateTimeOffset)this.GetValue(TimeProperty);
        set => this.SetValue(TimeProperty, value);
    }
	
	private static DateTimeOffset Min(DateTimeOffset x, DateTimeOffset y) => x < y ? x : y;
}
";
        RoslynAssert.Valid(Analyzer, code);
    }
}
