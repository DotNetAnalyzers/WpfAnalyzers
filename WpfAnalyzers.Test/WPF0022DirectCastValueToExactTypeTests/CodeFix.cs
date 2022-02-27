namespace WpfAnalyzers.Test.WPF0022DirectCastValueToExactTypeTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class CodeFix
{
    private static readonly CallbackAnalyzer Analyzer = new();
    private static readonly CastFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0022DirectCastValueToExactType);

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
            typeof(string),
            typeof(FooControl),
            new PropertyMetadata(1, OnValueChanged));

        public string Value
        {
            get { return (string)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓System.Collections.IEnumerable)e.NewValue;
        }
    }
}";

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Value is of type string"), code);
    }

    [TestCase("new PropertyMetadata(1, OnValueChanged)")]
    [TestCase("new PropertyMetadata(1, new PropertyChangedCallback(OnValueChanged))")]
    public static void DependencyPropertyRegisterPropertyChangedCallbackMethodGroup(string metadata)
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
            typeof(string),
            typeof(FooControl),
            new PropertyMetadata(default(string), OnValueChanged));

        public string Value
        {
            get { return (string)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓int)e.NewValue;
        }
    }
}".AssertReplace("new PropertyMetadata(default(string), OnValueChanged)", metadata);

        var after = @"
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
            new PropertyMetadata(default(string), OnValueChanged));

        public string Value
        {
            get { return (string)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (string)e.NewValue;
        }
    }
}".AssertReplace("new PropertyMetadata(default(string), OnValueChanged)", metadata);

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
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
            typeof(string),
            typeof(FooControl),
            new PropertyMetadata(default(string), OnValueChanged));

        public string Value
        {
            get => (string)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void OnValueChanged(object oldValue, object newValue)
        {
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FooControl)d).OnValueChanged(e.OldValue, (↓System.Collections.IEnumerable)e.NewValue);
        }
    }
}".AssertReplace("new PropertyMetadata(default(string), OnValueChanged)", $"new PropertyMetadata(1, {callback})");

        var after = @"
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
            new PropertyMetadata(default(string), OnValueChanged));

        public string Value
        {
            get => (string)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void OnValueChanged(object oldValue, object newValue)
        {
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FooControl)d).OnValueChanged(e.OldValue, (string)e.NewValue);
        }
    }
}".AssertReplace("new PropertyMetadata(default(string), OnValueChanged)", $"new PropertyMetadata(1, {callback})");

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [TestCase("(d, e) => OnValueChanged(d, e)")]
    [TestCase("new PropertyChangedCallback((d, e) => OnValueChanged(d, e))")]
    public static void DependencyPropertyRegisterPropertyChangedCallbackLambda(string lambda)
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
            typeof(string),
            typeof(FooControl),
            new PropertyMetadata(
                1,
                (d, e) => OnValueChanged(d, e)));

        public string Value
        {
            get => (string)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓System.Collections.IEnumerable)e.NewValue;
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
            typeof(string),
            typeof(FooControl),
            new PropertyMetadata(
                1,
                (d, e) => OnValueChanged(d, e)));

        public string Value
        {
            get => (string)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (string)e.NewValue;
        }
    }
}".AssertReplace("(d, e) => OnValueChanged(d, e)", lambda);

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [TestCase("(d, e) => ((FooControl)d).OnValueChanged((↓↓System.Collections.IEnumerable?)e.OldValue, (string?)e.NewValue)")]
    [TestCase("(d, e) => ((FooControl)d).OnValueChanged((string?)e.OldValue, (↓↓System.Collections.IEnumerable?)e.NewValue)")]
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
            typeof(string),
            typeof(FooControl),
            new PropertyMetadata(
                default(string),
                (d, e) => ((FooControl)d).OnValueChanged((string?)e.OldValue, (string?)e.NewValue)));

        public string? Value
        {
            get => (string?)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void OnValueChanged(object? oldValue, object? newValue)
        {
        }
    }
}".AssertReplace("(d, e) => ((FooControl)d).OnValueChanged((string?)e.OldValue, (string?)e.NewValue)", lambda);

        var after = @"
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
                (d, e) => ((FooControl)d).OnValueChanged((string?)e.OldValue, (string?)e.NewValue)));

        public string? Value
        {
            get => (string?)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void OnValueChanged(object? oldValue, object? newValue)
        {
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, after);
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
            typeof(string),
            typeof(FooControl),
            new PropertyMetadata(1, OnValueChanged, CoerceValue));

        public string Value
        {
            get { return (string)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            return (↓System.Collections.IEnumerable)baseValue;
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
            typeof(string),
            typeof(FooControl),
            new PropertyMetadata(1, OnValueChanged, CoerceValue));

        public string Value
        {
            get { return (string)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            return (string)baseValue;
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
            typeof(string),
            typeof(FooControl),
            new PropertyMetadata(1, OnValueChanged, CoerceValue),
            ValidateValue);

        public string Value
        {
            get { return (string)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            return (string)baseValue;
        }

        private static bool ValidateValue(object baseValue)
        {
            return ((↓System.Collections.IEnumerable)baseValue) != null;
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
            typeof(string),
            typeof(FooControl),
            new PropertyMetadata(1, OnValueChanged, CoerceValue),
            ValidateValue);

        public string Value
        {
            get { return (string)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            return (string)baseValue;
        }

        private static bool ValidateValue(object baseValue)
        {
            return ((string)baseValue) != null;
        }
    }
}".AssertReplace("ValidateValue);", validateValue + ");");

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [TestCase("(↓System.Collections.IEnumerable?)e.NewValue", "(string?)e.NewValue")]
    [TestCase("(↓System.Collections.IEnumerable?)e.OldValue", "(string?)e.OldValue")]
    public static void DependencyPropertyRegisterCast(string cast, string expectedCast)
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
            typeof(string),
            typeof(FooControl),
            new PropertyMetadata(default(string), OnValueChanged));

        public string? Value
        {
            get { return (string?)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓System.Collections.IEnumerable?)e.NewValue;
        }
    }
}".AssertReplace("(↓System.Collections.IEnumerable?)e.NewValue", cast);

        var after = @"
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
            new PropertyMetadata(default(string), OnValueChanged));

        public string? Value
        {
            get { return (string?)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (string?)e.NewValue;
        }
    }
}".AssertReplace("(string?)e.NewValue", expectedCast);

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
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
            typeof(string),
            typeof(FooControl),
            new PropertyMetadata(1.0, OnValueChanged));

        public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

        public string Value
        {
            get { return (string)this.GetValue(ValueProperty); }
            set { this.SetValue(ValuePropertyKey, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓System.Collections.IEnumerable)e.NewValue;
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
            typeof(string),
            typeof(FooControl),
            new PropertyMetadata(1.0, OnValueChanged));

        public static readonly DependencyProperty ValueProperty = ValuePropertyKey.DependencyProperty;

        public string Value
        {
            get { return (string)this.GetValue(ValueProperty); }
            set { this.SetValue(ValuePropertyKey, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (string)e.NewValue;
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
            typeof(string),
            typeof(Foo),
            new PropertyMetadata(1, OnValueChanged));

        public static void SetBar(this FrameworkElement element, string value) => element.SetValue(BarProperty, value);

        public static string GetBar(this FrameworkElement element) => (string)element.GetValue(BarProperty);

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓System.Collections.IEnumerable)e.NewValue;
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
            typeof(string),
            typeof(Foo),
            new PropertyMetadata(1, OnValueChanged));

        public static void SetBar(this FrameworkElement element, string value) => element.SetValue(BarProperty, value);

        public static string GetBar(this FrameworkElement element) => (string)element.GetValue(BarProperty);

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (string)e.NewValue;
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
            typeof(string),
            typeof(Foo),
            new PropertyMetadata(default(string), OnValueChanged));

            public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public static void SetBar(this FrameworkElement element, string value) => element.SetValue(BarPropertyKey, value);

        public static string? GetBar(this FrameworkElement element) => (string?)element.GetValue(BarProperty);

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (↓System.Collections.IEnumerable)e.NewValue;
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
            typeof(string),
            typeof(Foo),
            new PropertyMetadata(default(string), OnValueChanged));

            public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public static void SetBar(this FrameworkElement element, string value) => element.SetValue(BarPropertyKey, value);

        public static string? GetBar(this FrameworkElement element) => (string?)element.GetValue(BarProperty);

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (string?)e.NewValue;
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

        public static void SetBar(DependencyObject element, string value) => element.SetValue(BarProperty, value);

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
            var value = (↓System.Collections.IEnumerable)e.NewValue;
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
            var value = (↓System.Collections.IEnumerable)e.NewValue;
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