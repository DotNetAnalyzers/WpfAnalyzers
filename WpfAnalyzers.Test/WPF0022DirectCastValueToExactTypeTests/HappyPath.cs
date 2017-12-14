namespace WpfAnalyzers.Test.WPF0022DirectCastValueToExactTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class HappyPath
    {
        private static readonly CallbackMethodDeclarationAnalyzer Analyzer = new CallbackMethodDeclarationAnalyzer();

        [Test]
        public void DependencyPropertyRegisterNoMetadata()
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
            typeof(FooControl));

        public double Value
        {
            get { return (double)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("new PropertyMetadata(OnBarChanged)")]
        [TestCase("new PropertyMetadata(new PropertyChangedCallback(OnBarChanged))")]
        [TestCase("new PropertyMetadata(default(int), OnBarChanged)")]
        [TestCase("new PropertyMetadata(default(int), new PropertyChangedCallback(OnBarChanged))")]
        [TestCase("new PropertyMetadata((o, e) => { })")]
        [TestCase("new FrameworkPropertyMetadata((o, e) => { })")]
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
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
            var oldValue = (int)d.OldValue;
            var newValue = (int)d.NewValue;
        }

        private static object CoerceBar(DependencyObject d, object baseValue)
        {
            return baseValue;
        }
    }
}";
            testCode = testCode.AssertReplace("new PropertyMetadata(default(int), OnBarChanged)", metadata);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("int", "int")]
        [TestCase("System.IO.Stream", "IDisposable")]
        [TestCase("System.Collections.IEnumerable", "System.Collections.IEnumerable")]
        [TestCase("System.Collections.IList", "System.Collections.IEnumerable")]
        public void DependencyPropertyRegisterWithAllCallbacksDirectCast(string type, string toType)
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
            var oldValue = (string)d.OldValue;
            var newValue = (string)d.NewValue;
        }

        private static object CoerceValue(DependencyObject d, object basevalue)
        {
            return (string)basevalue;
        }

        private static bool ValidateValue(object basevalue)
        {
            return ((string)basevalue) != null;
        }
    }
}";

            testCode = testCode.AssertReplace("int", type);
            testCode = testCode.AssertReplace("string", type);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("object", "string")]
        [TestCase("object", "System.Collections.IEnumerable")]
        [TestCase("bool", "bool?")]
        [TestCase("System.IO.Stream", "System.IDisposable")]
        [TestCase("System.Collections.IEnumerable", "System.Collections.IEnumerable")]
        [TestCase("System.Collections.IEnumerable", "System.Collections.IList")]
        public void DependencyPropertyRegisterWithAllCallbacksAsCast(string type, string asType)
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
            new PropertyMetadata(default(int), OnValueChanged, CoerceValue),
            ValidateValue);

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FooControl;
            var oldValue = d.OldValue as string;
            var newValue = d.NewValue as string;
        }

        private static object CoerceValue(DependencyObject d, object basevalue)
        {
            return basevalue as string;
        }

        private static bool ValidateValue(object basevalue)
        {
            return (basevalue as string) != null;
        }
    }
}";
            testCode = testCode.AssertReplace("int", type);
            testCode = testCode.AssertReplace("string", asType);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("object", "string")]
        [TestCase("object", "System.Collections.IEnumerable")]
        [TestCase("bool", "bool?")]
        [TestCase("System.IO.Stream", "System.IDisposable")]
        [TestCase("System.Collections.IEnumerable", "System.Collections.IEnumerable")]
        [TestCase("System.Collections.IEnumerable", "System.Collections.IList")]
        public void DependencyPropertyRegisterWithAllCallbacksIsPatterns(string type, string isType)
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
            new PropertyMetadata(default(int), OnValueChanged, CoerceValue),
            ValidateValue);

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FooControl control)
            {
                if (e.OldValue is string oldText)
                {
                }
                if (e.NewValue is string newText)
                {
                }
            }
        }

        private static object CoerceValue(DependencyObject d, object basevalue)
        {
            if (d is FooControl control)
            {
                if (basevalue is string text)
                {
                }
            }

            return basevalue;
        }

        private static bool ValidateValue(object basevalue)
        {
            if (basevalue is string text)
            {
                return !string.IsNullOrWhiteSpace(text);
            }

            return false;
        }
    }
}";

            testCode = testCode.AssertReplace("int", type);
            testCode = testCode.AssertReplace("string", isType);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [TestCase("object", "string")]
        [TestCase("object", "System.Collections.IEnumerable")]
        [TestCase("bool", "bool?")]
        [TestCase("System.IO.Stream", "System.IDisposable")]
        [TestCase("System.Collections.IEnumerable", "System.Collections.IEnumerable")]
        [TestCase("System.Collections.IEnumerable", "System.Collections.IList")]
        public void DependencyPropertyRegisterWithAllCallbacksSwitchPatterns(string type, string caseType)
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
            new PropertyMetadata(default(int), OnValueChanged, CoerceValue),
            ValidateValue);

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }


        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            switch (d)
            {
                case FooControl control:
                    {
                        switch (e.OldValue)
                        {
                            case string oldText:
                                break;
                        }

                        switch (e.NewValue)
                        {
                            case string newText:
                                break;
                        }
                    }
                    break;
            }
        }

        private static object CoerceValue(DependencyObject d, object basevalue)
        {
            switch (d)
            {
                case FooControl control:
                {
                    switch (basevalue)
                    {
                        case string oldText:
                            break;
                    }
                }
                    break;
            }

            return basevalue;
        }

        private static bool ValidateValue(object basevalue)
        {
            switch (basevalue)
            {
                case string text:
                    return !string.IsNullOrWhiteSpace(text);
                default:
                    return false;
            }
        }
    }
}";
            testCode = testCode.AssertReplace("int", type);
            testCode = testCode.AssertReplace("string", caseType);
            AnalyzerAssert.Valid(Analyzer, testCode);
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
            typeof(double),
            typeof(FooControl),
            new PropertyMetadata(1.0, OnValueChanged));

        public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

        public double Value
        {
            get { return (double)this.GetValue(ValueProperty); }
            set { this.SetValue(ValuePropertyKey, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
            var oldValue = (int)d.OldValue;
            var newValue = (int)d.NewValue;
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

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
            new PropertyMetadata(default(int), OnBarChanged));

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
            var oldValue = (int)d.OldValue;
            var newValue = (int)d.NewValue;
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

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
            new PropertyMetadata(default(int), OnBarChanged));

            public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FrameworkElement)d;
            var oldValue = (int)d.OldValue;
            var newValue = (int)d.NewValue;
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

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
                new FrameworkPropertyMetadata(null, OnBackgroundChanged));
        }

        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
            var oldValue = (int)d.OldValue;
            var newValue = (int)d.NewValue;
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

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
            TextElement.FontSizeProperty.AddOwner(typeof(FooControl), new PropertyMetadata(12.0, OnFontSizeChanged));
        }

        private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
            var oldValue = (int)d.OldValue;
            var newValue = (int)d.NewValue;
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}