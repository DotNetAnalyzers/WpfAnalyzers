﻿namespace WpfAnalyzers.Test.WPF0020CastValueToCorrectTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class Diagnostic
    {
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("WPF0020");

        [Test]
        public void Message()
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
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(1, OnValueChanged));

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓string)e.NewValue;
        }
    }
}";

            var expectedDiagnostic = ExpectedDiagnostic.Create(
                "WPF0020",
                "Value is of type int.");
            AnalyzerAssert.Diagnostics<CallbackMethodDeclarationAnalyzer>(expectedDiagnostic, testCode);
        }

        [TestCase("new PropertyMetadata(1, OnValueChanged)")]
        [TestCase("new PropertyMetadata(1, new PropertyChangedCallback(OnValueChanged))")]
        public void DependencyPropertyRegisterPropertyChangedCallback(string metadata)
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
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(1, OnValueChanged));

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓string)e.NewValue;
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
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(1, OnValueChanged));

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (int)e.NewValue;
        }
    }
}";
            testCode = testCode.AssertReplace("new PropertyMetadata(1, OnValueChanged)", metadata);
            fixedCode = fixedCode.AssertReplace("new PropertyMetadata(1, OnValueChanged)", metadata);

            AnalyzerAssert.CodeFix<CallbackMethodDeclarationAnalyzer, FixCastCodeFixProvider>(ExpectedDiagnostic, testCode, fixedCode);
        }

        [TestCase("new PropertyMetadata(1, OnValueChanged, CoerceValue)")]
        [TestCase("new PropertyMetadata(1, new PropertyChangedCallback(OnValueChanged), new CoerceValueCallback(CoerceValue))")]
        public void DependencyPropertyRegisterCoerceValueCallback(string metadata)
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
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(1, OnValueChanged, CoerceValue));

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
        }

        private static object CoerceValue(DependencyObject d, object basevalue)
        {
            return (↓string)basevalue;
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
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(1, OnValueChanged, CoerceValue));

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
        }

        private static object CoerceValue(DependencyObject d, object basevalue)
        {
            return (int)basevalue;
        }
    }
}";
            testCode = testCode.AssertReplace("new PropertyMetadata(1, OnValueChanged, CoerceValue)", metadata);
            fixedCode = fixedCode.AssertReplace("new PropertyMetadata(1, OnValueChanged, CoerceValue)", metadata);
            AnalyzerAssert.CodeFix<CallbackMethodDeclarationAnalyzer, FixCastCodeFixProvider>(ExpectedDiagnostic, testCode, fixedCode);
        }

        [TestCase("(↓string)e.NewValue", "(int)e.NewValue")]
        [TestCase("(↓string)e.OldValue", "(int)e.OldValue")]
        public void DependencyPropertyRegisterCast(string fromCast, string toCast)
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
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(1, OnValueChanged));

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓string)e.NewValue;
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
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(1, OnValueChanged));

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (int)e.NewValue;
        }
    }
}";
            testCode = testCode.AssertReplace("(↓string)e.NewValue", fromCast);
            fixedCode = fixedCode.AssertReplace("(int)e.NewValue", toCast);

            AnalyzerAssert.CodeFix<CallbackMethodDeclarationAnalyzer, FixCastCodeFixProvider>(ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void DependencyPropertyRegisterReadOnly()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static readonly DependencyPropertyKey ValuePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Value),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(1.0, OnValueChanged));

        public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValuePropertyKey, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓string)e.NewValue;
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
        private static readonly DependencyPropertyKey ValuePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Value),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(1.0, OnValueChanged));

        public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValuePropertyKey, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (int)e.NewValue;
        }
    }
}";
            AnalyzerAssert.CodeFix<CallbackMethodDeclarationAnalyzer, FixCastCodeFixProvider>(ExpectedDiagnostic, testCode, fixedCode);
        }

        [Explicit("Not handling this yet.")]
        [Test]
        public void DependencyPropertyRegisterAttached()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(1, OnValueChanged));

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓string)e.NewValue;
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(1, OnBarChanged));

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓string)e.NewValue;
        }
    }
}";
            AnalyzerAssert.CodeFix<CallbackMethodDeclarationAnalyzer, FixCastCodeFixProvider>(ExpectedDiagnostic, testCode, fixedCode);
        }

        [Explicit("Not handling this yet.")]
        [Test]
        public void DependencyPropertyRegisterAttachedReadOnly()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int), OnValueChanged));

            public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓string)e.NewValue;
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int), OnBarChanged));

            public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓string)e.NewValue;
        }
    }
}";
            AnalyzerAssert.CodeFix<CallbackMethodDeclarationAnalyzer, FixCastCodeFixProvider>(ExpectedDiagnostic, testCode, fixedCode);
        }

        [Explicit("Not handling this yet.")]
        [Test]
        public void DependencyPropertyOverrideMetadata()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : UserControl
    {
        static FooControl()
        {
            BackgroundProperty.OverrideMetadata(typeof(FooControl),
                new FrameworkPropertyMetadata(null, OnValueChanged));
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓string)e.NewValue;
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : UserControl
    {
        static FooControl()
        {
            BackgroundProperty.OverrideMetadata(typeof(FooControl),
                new FrameworkPropertyMetadata(null, OnBackgroundChanged));
        }

        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓string)e.NewValue;
        }
    }
}";
            AnalyzerAssert.CodeFix<CallbackMethodDeclarationAnalyzer, FixCastCodeFixProvider>(ExpectedDiagnostic, testCode, fixedCode);
        }

        [Explicit("Not handling this yet.")]
        [Test]
        public void DependencyPropertyAddOwner()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    public class FooControl : FrameworkElement
    {
        static FooControl()
        {
            TextElement.FontSizeProperty.AddOwner(typeof(FooControl), new PropertyMetadata(12.0, OnValueChanged));
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓string)e.NewValue;
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    public class FooControl : FrameworkElement
    {
        static FooControl()
        {
            TextElement.FontSizeProperty.AddOwner(typeof(FooControl), new PropertyMetadata(12.0, OnFontSizeChanged));
        }

        private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";
            AnalyzerAssert.CodeFix<CallbackMethodDeclarationAnalyzer, FixCastCodeFixProvider>(ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}