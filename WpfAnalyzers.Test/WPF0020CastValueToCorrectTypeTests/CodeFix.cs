namespace WpfAnalyzers.Test.WPF0020CastValueToCorrectTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly CallbackAnalyzer Analyzer = new();
        private static readonly CastFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0020CastValueToCorrectType);

        [Test]
        public static void Message()
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

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Value is of type int"), code);
        }

        [TestCase("OnValueChanged")]
        [TestCase("new PropertyChangedCallback(OnValueChanged)")]
        public static void DependencyPropertyRegisterPropertyChangedCallbackMethodGroup(string callback)
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
}".AssertReplace("new PropertyMetadata(1, OnValueChanged)", $"new PropertyMetadata(1, {callback})");

            var after = @"
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
}".AssertReplace("new PropertyMetadata(1, OnValueChanged)", $"new PropertyMetadata(1, {callback})");

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("(d, e) => OnValueChanged(d, e)")]
        [TestCase("new PropertyChangedCallback((d, e) => OnValueChanged(d, e))")]
        public static void DependencyPropertyRegisterPropertyChangedCallbackLambdaCallingStatic(string lambda)
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
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(
                1,
                (d, e) => OnValueChanged(d, e)));

        public int Value
        {
            get => (int)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓string)e.NewValue;
        }
    }
}".AssertReplace("(d, e) => OnValueChanged(d, e)", lambda);

            var after = @"
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
            new PropertyMetadata(
                1,
                (d, e) => OnValueChanged(d, e)));

        public int Value
        {
            get => (int)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (int)e.NewValue;
        }
    }
}".AssertReplace("(d, e) => OnValueChanged(d, e)", lambda);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("(d, e) => ((FooControl)d).OnValueChanged((↓string)e.OldValue, (int)e.NewValue)")]
        [TestCase("(d, e) => ((FooControl)d).OnValueChanged((int)e.OldValue, (↓string)e.NewValue)")]
        public static void DependencyPropertyRegisterPropertyChangedCallbackLambdaCallingInstanceMethod(string lambda)
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
            new PropertyMetadata(
                default(int),
                (d, e) => ((FooControl)d).OnValueChanged((int)e.OldValue, (int)e.NewValue)));

        public int Value
        {
            get => (int)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void OnValueChanged(object oldValue, object newValue)
        {
        }
    }
}".AssertReplace("(d, e) => ((FooControl)d).OnValueChanged((int)e.OldValue, (int)e.NewValue)", lambda);

            var after = @"
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
            new PropertyMetadata(
                default(int),
                (d, e) => ((FooControl)d).OnValueChanged((int)e.OldValue, (int)e.NewValue)));

        public int Value
        {
            get => (int)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void OnValueChanged(object oldValue, object newValue)
        {
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, after);
        }

        [TestCase("OnValueChanged")]
        [TestCase("new PropertyChangedCallback(OnValueChanged)")]
        public static void DependencyPropertyRegisterPropertyChangedCallbackMethodGroupCallingInstanceMethod(string callback)
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
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(1, OnValueChanged));

        public int Value
        {
            get => (int)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void OnValueChanged(object oldValue, object newValue)
        {
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FooControl)d).OnValueChanged(e.OldValue, (↓string)e.NewValue);
        }
    }
}".AssertReplace("new PropertyMetadata(1, OnValueChanged)", $"new PropertyMetadata(1, {callback})");

            var after = @"
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
            new PropertyMetadata(1, OnValueChanged));

        public int Value
        {
            get => (int)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void OnValueChanged(object oldValue, object newValue)
        {
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FooControl)d).OnValueChanged(e.OldValue, (int)e.NewValue);
        }
    }
}".AssertReplace("new PropertyMetadata(1, OnValueChanged)", $"new PropertyMetadata(1, {callback})");

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("new PropertyMetadata(1, OnValueChanged, CoerceValue)")]
        [TestCase("new PropertyMetadata(1, new PropertyChangedCallback(OnValueChanged), new CoerceValueCallback(CoerceValue))")]
        public static void DependencyPropertyRegisterCoerceValueCallback(string metadata)
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

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            return (↓string)baseValue;
        }
    }
}".AssertReplace("new PropertyMetadata(1, OnValueChanged, CoerceValue)", metadata);

            var after = @"
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

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            return (int)baseValue;
        }
    }
}".AssertReplace("new PropertyMetadata(1, OnValueChanged, CoerceValue)", metadata);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("ValidateValue")]
        [TestCase("new ValidateValueCallback(ValidateValue)")]
        public static void DependencyPropertyRegisterValidateValue(string validateValue)
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
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(1, OnValueChanged, CoerceValue),
            ValidateValue);

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            return (int)baseValue;
        }

        private static bool ValidateValue(object baseValue)
        {
            return ((↓string)baseValue) > 1;
        }
    }
}".AssertReplace("ValidateValue);", validateValue + ");");

            var after = @"
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
            new PropertyMetadata(1, OnValueChanged, CoerceValue),
            ValidateValue);

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            return (int)baseValue;
        }

        private static bool ValidateValue(object baseValue)
        {
            return ((int)baseValue) > 1;
        }
    }
}".AssertReplace("ValidateValue);", validateValue + ");");

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("(↓string)e.NewValue", "(int)e.NewValue")]
        [TestCase("(↓string)e.OldValue", "(int)e.OldValue")]
        public static void DependencyPropertyRegisterPropertyChangedCallback(string fromCast, string toCast)
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
}".AssertReplace("(↓string)e.NewValue", fromCast);

            var after = @"
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
}".AssertReplace("(int)e.NewValue", toCast);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("int", "string")]
        public static void DependencyPropertyRegisterWithAllCallbacksIsPatterns(string type, string isType)
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
                if (e.OldValue is ↓string oldText)
                {
                }
                if (e.NewValue is ↓string newText)
                {
                }
            }
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            if (d is FooControl control)
            {
                if (baseValue is ↓string text)
                {
                }
            }

            return baseValue;
        }

        private static bool ValidateValue(object baseValue)
        {
            if (baseValue is ↓string text)
            {
                return !string.IsNullOrWhiteSpace(text);
            }

            return false;
        }
    }
}".AssertReplace("int", type)
  .AssertReplace("string", isType);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("int", "string")]
        public static void DependencyPropertyRegisterWithAllCallbacksSwitchPatterns(string type, string caseType)
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
                            case ↓string oldText:
                                break;
                        }

                        switch (e.NewValue)
                        {
                            case ↓string newText:
                                break;
                        }
                    }
                    break;
            }
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            switch (d)
            {
                case FooControl control:
                {
                    switch (baseValue)
                    {
                        case ↓string oldText:
                            break;
                    }
                }
                    break;
            }

            return baseValue;
        }

        private static bool ValidateValue(object baseValue)
        {
            switch (baseValue)
            {
                case ↓string text:
                    return !string.IsNullOrWhiteSpace(text);
                default:
                    return false;
            }
        }
    }
}".AssertReplace("int", type)
  .AssertReplace("string", caseType);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void DependencyPropertyRegisterReadOnly()
        {
            var before = @"
namespace N
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

            var after = @"
namespace N
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyRegisterAttached()
        {
            var before = @"
namespace N
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

            var after = @"
namespace N
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
            var value = (int)e.NewValue;
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void DependencyPropertyRegisterAttachedReadOnly()
        {
            var before = @"
namespace N
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

            var after = @"
namespace N
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
            var value = (int)e.NewValue;
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
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
            typeof(string), 
            typeof(Foo), 
            new FrameworkPropertyMetadata(
                default(string), 
                FrameworkPropertyMetadataOptions.Inherits));

        public static void SetBar(DependencyObject element, string? value) => element.SetValue(BarProperty, value);

        public static string? GetBar(DependencyObject element) => (string?)element.GetValue(BarProperty);
    }
}";

            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(
            typeof(FooControl), 
            new FrameworkPropertyMetadata(
                default(string), 
                OnBarChanged));

        public string? Bar
        {
            get => (string?)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓int)e.NewValue;
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
        public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(
            typeof(FooControl), 
            new FrameworkPropertyMetadata(
                default(string), 
                OnBarChanged));

        public string? Bar
        {
            get => (string?)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (string?)e.NewValue;
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { fooCode, before }, after);
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
            nameof(Value),
            typeof(string),
            typeof(FooControl),
            new PropertyMetadata(default(string)));

        public string? Value
        {
            get => (string?)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }
    }
}";

            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class BarControl : FooControl
    {
        static BarControl()
        {
            ValueProperty.OverrideMetadata(typeof(BarControl), new PropertyMetadata(default(string), OnValueChanged));
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓int)e.NewValue;
        }
    }
}";

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class BarControl : FooControl
    {
        static BarControl()
        {
            ValueProperty.OverrideMetadata(typeof(BarControl), new PropertyMetadata(default(string), OnValueChanged));
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (string?)e.NewValue;
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { fooControlCode, before }, after);
        }
    }
}
