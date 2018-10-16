namespace WpfAnalyzers.Test.WPF0005PropertyChangedCallbackShouldMatchRegisteredNameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal partial class CodeFix
    {
        internal class InstanceCallback
        {
            private static readonly DiagnosticAnalyzer Analyzer = new CallbackAnalyzer();
            private static readonly CodeFixProvider Fix = new RenameMemberCodeFixProvider();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0005PropertyChangedCallbackShouldMatchRegisteredName.Descriptor);

            [TestCase("(d, e) => ((FooControl)d).WrongName((double)e.OldValue, (double)e.NewValue)", "(d, e) => ((FooControl)d).OnValueChanged((double)e.OldValue, (double)e.NewValue)")]
            [TestCase("new PropertyChangedCallback((d, e) => ((FooControl)d).WrongName((double)e.OldValue, (double)e.NewValue))", "new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.OldValue, (double)e.NewValue))")]
            public void DependencyPropertyRegisterInstanceMethodOldBeforeNew(string before, string after)
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
}".AssertReplace("(d, e) => ((FooControl)d).WrongName((double)e.OldValue, (double)e.NewValue)", before);

                var fixedCode = @"
namespace RoslynSandbox
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
}".AssertReplace("(d, e) => ((FooControl)d).OnValueChanged((double)e.OldValue, (double)e.NewValue)", after);

                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [TestCase("(d, e) => ((FooControl)d).WrongName((double)e.NewValue, (double)e.OldValue)", "(d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue)")]
            [TestCase("new PropertyChangedCallback((d, e) => ((FooControl)d).WrongName((double)e.NewValue, (double)e.OldValue))", "new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue))")]
            public void DependencyPropertyRegisterInstanceMethodNewBeforeOld(string before, string after)
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
}".AssertReplace("(d, e) => ((FooControl)d).WrongName((double)e.NewValue, (double)e.OldValue)", before);

                var fixedCode = @"
namespace RoslynSandbox
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
}".AssertReplace("(d, e) => ((FooControl)d).OnValueChanged((double)e.NewValue, (double)e.OldValue)", after);

                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public void DependencyPropertyRegisterWithMetadataStaticCallbackCallingInstance()
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
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public void DependencyPropertyRegisterWithMetadataStaticCallbackCallingInstanceOnTempCast()
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
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public void DependencyPropertyRegisterWithMetadataStaticCallbackCallingInstanceOnTempPatternMathc()
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
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }
        }
    }
}
