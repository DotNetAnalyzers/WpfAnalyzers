namespace WpfAnalyzers.Test.WPF0062DocumentPropertyChangedCallbackTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new CallbackAnalyzer();
        private static readonly CodeFixProvider Fix = new DocumentOnPropertyChangedFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0062DocumentPropertyChangedCallback.Descriptor);

        [TestCase("new PropertyMetadata(OnBarChanged)")]
        [TestCase("new PropertyMetadata(new PropertyChangedCallback(OnBarChanged))")]
        [TestCase("new PropertyMetadata(default(int), OnBarChanged)")]
        [TestCase("new PropertyMetadata(default(int), new PropertyChangedCallback(OnBarChanged))")]
        [TestCase("new FrameworkPropertyMetadata(OnBarChanged)")]
        [TestCase("new FrameworkPropertyMetadata(OnBarChanged, CoerceBar)")]
        public void DependencyPropertyRegisterWithMetadata(string metadata)
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

            var fixedCode = @"
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

            RoslynAssert.Valid(Analyzer, fixedCode);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void DependencyPropertyRegisterWithMetadataDifferentParameterNames()
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

            var fixedCode = @"
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

            RoslynAssert.Valid(Analyzer, fixedCode);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void DependencyPropertyRegisterMissingDocsOldValueAndNewValue()
        {
            var testCode = @"
namespace RoslynSandbox
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

            var fixedCode = @"
namespace RoslynSandbox
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
            RoslynAssert.Valid(Analyzer, fixedCode);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void DependencyPropertyRegisterSummaryNotStandard()
        {
            var testCode = @"
namespace RoslynSandbox
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

        /// ↓<summary>Not standard.</summary>
        /// <param name=""oldValue"">The old value of <see cref=""ValueProperty""/>.</param>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
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
            RoslynAssert.Valid(Analyzer, fixedCode);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void DependencyPropertyRegisterOldValueNotStandard()
        {
            var testCode = @"
namespace RoslynSandbox
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
        /// ↓<param name=""oldValue"">Not standard.</param>
        /// <param name=""newValue"">The new value of <see cref=""ValueProperty""/>.</param>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
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
            RoslynAssert.Valid(Analyzer, fixedCode);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void DependencyPropertyRegisterNewValueNotStandard()
        {
            var testCode = @"
namespace RoslynSandbox
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
        /// ↓<param name=""newValue"">Not standard.</param>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
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
            RoslynAssert.Valid(Analyzer, fixedCode);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void DependencyPropertyRegisterMissingDocsNewValueBeforeOldValue()
        {
            var testCode = @"
namespace RoslynSandbox
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

            var fixedCode = @"
namespace RoslynSandbox
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
            RoslynAssert.Valid(Analyzer, fixedCode);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void DependencyPropertyRegisterMissingDocsOnlyNewValue()
        {
            var testCode = @"
namespace RoslynSandbox
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

        protected virtual void OnValueChanged(double newValue)
        {
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
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
            RoslynAssert.Valid(Analyzer, fixedCode);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void DependencyPropertyRegisterMissingDocsNoParameters()
        {
            var testCode = @"
namespace RoslynSandbox
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

        protected virtual void OnValueChanged()
        {
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
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
            RoslynAssert.Valid(Analyzer, fixedCode);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void DependencyPropertyRegisterMissingDocsExplicitCallback()
        {
            var testCode = @"
namespace RoslynSandbox
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

            var fixedCode = @"
namespace RoslynSandbox
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
            RoslynAssert.Valid(Analyzer, fixedCode);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void DependencyPropertyRegisterStaticCallbackCallingInstance()
        {
            var testCode = @"
namespace RoslynSandbox
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

            var fixedCode = @"
namespace RoslynSandbox
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
            RoslynAssert.Valid(Analyzer, fixedCode);
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}
