namespace WpfAnalyzers.Test.WPF0062DocumentPropertyChangedCallbackTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new CallbackAnalyzer();
        private static readonly CodeFixProvider Fix = new DocumentationFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0062DocumentPropertyChangedCallback);

        [TestCase("new PropertyMetadata(OnBarChanged)")]
        [TestCase("new PropertyMetadata(new PropertyChangedCallback(OnBarChanged))")]
        [TestCase("new PropertyMetadata(default(int), OnBarChanged)")]
        [TestCase("new PropertyMetadata(default(int), new PropertyChangedCallback(OnBarChanged))")]
        [TestCase("new FrameworkPropertyMetadata(OnBarChanged)")]
        [TestCase("new FrameworkPropertyMetadata(OnBarChanged, CoerceBar)")]
        public static void DependencyPropertyRegisterWithMetadata(string metadata)
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
            new PropertyMetadata(default(int), OnBarChanged));

        public int Bar
        {
            get => (int)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }

        protected virtual void ↓OnBarChanged(object oldValue, object newValue)
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
}".AssertReplace("new PropertyMetadata(default(int), OnBarChanged)", metadata);

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
            new PropertyMetadata(default(int), OnBarChanged));

        public int Bar
        {
            get => (int)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }

        /// <summary>This method is invoked when the <see cref=""BarProperty""/> changes.</summary>
        /// <param name=""oldValue"">The old value of <see cref=""BarProperty""/>.</param>
        /// <param name=""newValue"">The new value of <see cref=""BarProperty""/>.</param>
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
}".AssertReplace("new PropertyMetadata(default(int), OnBarChanged)", metadata);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyRegisterWithMetadataDifferentParameterNames()
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
            new PropertyMetadata(default(int), OnBarChanged));

        public int Bar
        {
            get => (int)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }

        protected virtual void ↓OnBarChanged(object oldBar, object newBar)
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
            new PropertyMetadata(default(int), OnBarChanged));

        public int Bar
        {
            get => (int)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }

        /// <summary>This method is invoked when the <see cref=""BarProperty""/> changes.</summary>
        /// <param name=""oldBar"">The old value of <see cref=""BarProperty""/>.</param>
        /// <param name=""newBar"">The new value of <see cref=""BarProperty""/>.</param>
        protected virtual void OnBarChanged(object oldBar, object newBar)
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
        public static void DependencyPropertyRegisterMissingDocsOldValueAndNewValue()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
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

        protected virtual void ↓OnValueChanged(double oldValue, double newValue)
        {
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
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
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

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyRegisterSummaryNotStandard()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
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

        /// <summary>↓Not standard.</summary>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
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
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
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

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyRegisterOldValueNotStandard()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
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

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""oldValue"">↓Not standard.</param>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
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
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
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

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyRegisterNewValueNotStandard()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
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

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""newValue"">↓Not standard.</param>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
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
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
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

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyRegisterMissingDocsNewValueBeforeOldValue()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void ↓OnValueChanged(double newValue, double oldValue)
        {
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
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double newValue, double oldValue)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyRegisterMissingDocsOnlyNewValue()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void ↓OnValueChanged(double newValue)
        {
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
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double newValue)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyRegisterMissingDocsNoParameters()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged())));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void ↓OnValueChanged()
        {
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
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged())));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        protected virtual void OnValueChanged()
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyRegisterMissingDocsExplicitCallback()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.OldValue, (double)e.NewValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void ↓OnValueChanged(double oldValue, double newValue)
        {
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
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.OldValue, (double)e.NewValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyRegisterStaticCallbackCallingInstance()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback(OnValueChanged)));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void ↓OnValueChanged(double oldValue, double newValue)
        {
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FooControl)d).OnValueChanged((double)e.OldValue, (double)e.NewValue);
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
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback(OnValueChanged)));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FooControl)d).OnValueChanged((double)e.OldValue, (double)e.NewValue);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void MissingSummary()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        ///↓ <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double newValue, double oldValue)
        {
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
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double newValue, double oldValue)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WrongSummary()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        /// <summary>↓WRONG</summary>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double newValue, double oldValue)
        {
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
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double newValue, double oldValue)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WrongSummaryMultiline()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        /// <summary>
        /// ↓WRONG
        /// </summary>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double newValue, double oldValue)
        {
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
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double newValue, double oldValue)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void MissingOldValue()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double newValue, double ↓oldValue)
        {
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
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double newValue, double oldValue)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void MissingNewValue()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.OldValue, (double)e.NewValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double oldValue, double ↓newValue)
        {
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
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.OldValue, (double)e.NewValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WrongCrefNewValue()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.OldValue, (double)e.NewValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""newValue"">The new value of <see cref=""↓WRONG""/>.</param>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
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
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.OldValue, (double)e.NewValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void NotCrefNewValue()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.OldValue, (double)e.NewValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""newValue"">↓The new value of <paramref name=""oldValue""/>.</param>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
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
        /// <summary>Identifies the <see cref=""Value""/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(
                default(double),
                new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.OldValue, (double)e.NewValue))));

        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        /// <summary>This method is invoked when the <see cref=""ValueProperty""/> changes.</summary>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
