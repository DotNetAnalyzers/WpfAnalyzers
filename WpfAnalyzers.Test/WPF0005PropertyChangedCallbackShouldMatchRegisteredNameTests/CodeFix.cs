﻿namespace WpfAnalyzers.Test.WPF0005PropertyChangedCallbackShouldMatchRegisteredNameTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static partial class CodeFix
{
    public static class InstanceCallback
    {
        private static readonly CallbackAnalyzer Analyzer = new();
        private static readonly RenameMemberFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0005PropertyChangedCallbackShouldMatchRegisteredName);

        [TestCase("(d, e) => ((FooControl)d).WrongName((double)e.OldValue, (double)e.NewValue)",                              "(d, e) => ((FooControl)d).OnValueChanged((double)e.OldValue, (double)e.NewValue)")]
        [TestCase("new PropertyChangedCallback((d, e) => ((FooControl)d).WrongName((double)e.OldValue, (double)e.NewValue))", "new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.OldValue, (double)e.NewValue))")]
        public static void DependencyPropertyRegisterInstanceMethodOldBeforeNew(string lambdaBefore, string lambdaAfter)
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                (d, e) => ((FooControl)d).WrongName((double)e.OldValue, (double)e.NewValue)));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void ↓WrongName(double oldValue, double newValue)
        {
        }
    }
}".AssertReplace("(d, e) => ((FooControl)d).WrongName((double)e.OldValue, (double)e.NewValue)", lambdaBefore);

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                (d, e) => ((FooControl)d).OnValueChanged((double)e.OldValue, (double)e.NewValue)));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
        }
    }
}".AssertReplace("(d, e) => ((FooControl)d).OnValueChanged((double)e.OldValue, (double)e.NewValue)", lambdaAfter);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("(d, e) => ((FooControl)d).WrongName((double)e.NewValue, (double)e.OldValue)",                              "(d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue)")]
        [TestCase("new PropertyChangedCallback((d, e) => ((FooControl)d).WrongName((double)e.NewValue, (double)e.OldValue))", "new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue))")]
        public static void DependencyPropertyRegisterInstanceMethodNewBeforeOld(string lambdaBefore, string lambdaAfter)
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(default(double), (d, e) => ((FooControl)d).WrongName((double)e.NewValue, (double)e.OldValue)));

        public double Value
        {
            get { return (double)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        protected virtual void ↓WrongName(double newValue, double oldValue)
        {
        }
    }
}".AssertReplace("(d, e) => ((FooControl)d).WrongName((double)e.NewValue, (double)e.OldValue)", lambdaBefore);

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(default(double), (d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue)));

        public double Value
        {
            get { return (double)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        protected virtual void OnValueChanged(double newValue, double oldValue)
        {
        }
    }
}".AssertReplace("(d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue)", lambdaAfter);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyRegisterWithMetadataStaticCallbackCallingInstance()
        {
            var before = @"
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
            new PropertyMetadata(default(int), OnBarChanged, CoerceBar));

        public int Bar
        {
            get => (int)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }

        protected virtual void ↓WrongName(int oldValue, int newValue)
        {
        }

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FooControl)d).WrongName((int)e.OldValue, (int)e.NewValue);
        }

        private static object CoerceBar(DependencyObject d, object baseValue)
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

            var after = @"
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
            new PropertyMetadata(default(int), OnBarChanged, CoerceBar));

        public int Bar
        {
            get => (int)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }

        protected virtual void OnBarChanged(int oldValue, int newValue)
        {
        }

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FooControl)d).OnBarChanged((int)e.OldValue, (int)e.NewValue);
        }

        private static object CoerceBar(DependencyObject d, object baseValue)
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyRegisterWithMetadataStaticCallbackCallingInstanceOnTempCast()
        {
            var before = @"
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
            new PropertyMetadata(default(int), OnBarChanged, CoerceBar));

        public int Bar
        {
            get => (int)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }

        protected virtual void ↓WrongName(object oldValue, object newValue)
        {
        }

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
            control.WrongName(e.OldValue, e.NewValue);
        }

        private static object CoerceBar(DependencyObject d, object baseValue)
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

            var after = @"
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
            new PropertyMetadata(default(int), OnBarChanged, CoerceBar));

        public int Bar
        {
            get => (int)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }

        protected virtual void OnBarChanged(object oldValue, object newValue)
        {
        }

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
            control.OnBarChanged(e.OldValue, e.NewValue);
        }

        private static object CoerceBar(DependencyObject d, object baseValue)
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyRegisterWithMetadataStaticCallbackCallingInstanceOnTempPatternMathc()
        {
            var before = @"
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
            new PropertyMetadata(default(int), OnBarChanged, CoerceBar));

        public int Bar
        {
            get => (int)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }

        protected virtual void ↓WrongName(object oldValue, object newValue)
        {
        }

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FooControl control)
            {
                control.WrongName(e.OldValue, e.NewValue);
            }
        }

        private static object CoerceBar(DependencyObject d, object baseValue)
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

            var after = @"
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
            new PropertyMetadata(default(int), OnBarChanged, CoerceBar));

        public int Bar
        {
            get => (int)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }

        protected virtual void OnBarChanged(object oldValue, object newValue)
        {
        }

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FooControl control)
            {
                control.OnBarChanged(e.OldValue, e.NewValue);
            }
        }

        private static object CoerceBar(DependencyObject d, object baseValue)
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
