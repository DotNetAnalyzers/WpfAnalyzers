﻿namespace WpfAnalyzers.Test.WPF0023ConvertToLambdaTests;

using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

[TestFixture(typeof(RegistrationAnalyzer))]
[TestFixture(typeof(PropertyMetadataAnalyzer))]
public static class Valid<T>
    where T : DiagnosticAnalyzer, new()
{
    private static readonly DiagnosticAnalyzer Analyzer = new T();
    //// ReSharper disable once StaticMemberInGenericType
    private static readonly DiagnosticDescriptor Descriptor = Descriptors.WPF0023ConvertToLambda;

    [Test]
    public static void DependencyPropertyRegisterPropertyChangedCallbackLambdaCallingInstanceMethod()
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
            typeof(FooControl),
            new PropertyMetadata(
                default(string),
                (d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue)));

        public string Value
        {
            get => (string)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void OnValueChanged(object oldValue, object newValue)
        {
        }
    }
}";

        RoslynAssert.Valid(Analyzer, Descriptor, code);
    }

    [TestCase("new PropertyMetadata(OnBarChanged)")]
    [TestCase("new PropertyMetadata(new PropertyChangedCallback(OnBarChanged))")]
    [TestCase("new PropertyMetadata(default(int), OnBarChanged)")]
    [TestCase("new PropertyMetadata(default(int), new PropertyChangedCallback(OnBarChanged))")]
    [TestCase("new PropertyMetadata((o, e) => { })")]
    [TestCase("new FrameworkPropertyMetadata((o, e) => { })")]
    [TestCase("new FrameworkPropertyMetadata(OnBarChanged)")]
    [TestCase("new FrameworkPropertyMetadata(OnBarChanged, CoerceBar)")]
    public static void DependencyPropertyRegisterWithMetadata(string metadata)
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
            new PropertyMetadata(default(int), OnBarChanged));

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
            var oldValue = (int)e.OldValue;
            var newValue = (int)e.NewValue;
        }

        private static object? CoerceBar(DependencyObject d, object? baseValue)
        {
            var o = baseValue;
            return o;
        }
    }
}".AssertReplace("new PropertyMetadata(default(int), OnBarChanged)", metadata);

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void DependencyPropertyRegisterWithAllCallbacksMoreThanOneStatement()
    {
        var code = @"
#nullable disable
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int), OnValueChanged, CoerceValue),
            ValidateValue);

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
            var oldValue = (int)e.OldValue;
            var newValue = (int)e.NewValue;
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            var control = (FooControl)d;
            var value = (int)baseValue;
            return value;
        }

        private static bool ValidateValue(object baseValue)
        {
            var notOne = ((int)baseValue) != 1;
            return notOne;
        }
    }
}";

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void DependencyPropertyRegisterOnPropertyChangedIf()
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
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int), OnValueChanged));

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FooControl control)
            {
            }
        }
    }
}";

        RoslynAssert.Valid(Analyzer, code);
    }

    [TestCase("protected")]
    [TestCase("protected virtual")]
    [TestCase("internal")]
    [TestCase("public")]
    public static void IgnoreProtectedVirtualInstanceMethod(string modifiers)
    {
        var code = @"
namespace N
{
    using System.Windows;

    public class WithProtectedInstanceCallback : FrameworkElement
    {
        /// <summary>Identifies the <see cref=""Number""/> dependency property.</summary>
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            nameof(Number),
            typeof(int),
            typeof(WithProtectedInstanceCallback),
            new PropertyMetadata(
                default(int),
                (d, e) => ((WithProtectedInstanceCallback)d).OnNumberChanged()));

        private int n;

        public int Number
        {
            get => (int)this.GetValue(NumberProperty);
            set => this.SetValue(NumberProperty, value);
        }

        public int N => this.n;

        /// <summary>This method is invoked when the <see cref=""NumberProperty""/> changes.</summary>
        protected virtual void OnNumberChanged()
        {
            n = 0;
        }
    }
}".AssertReplace("protected virtual", modifiers);

        RoslynAssert.Valid(Analyzer, code);
    }
}
