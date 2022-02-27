namespace WpfAnalyzers.Test.WPF0023ConvertToLambdaTests;

using Gu.Roslyn.Asserts;

using NUnit.Framework;

public static class CodeFix
{
    private static readonly PropertyMetadataAnalyzer Analyzer = new();
    private static readonly ConvertToLambdaFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0023ConvertToLambda);

    [TestCase("OnValueChanged",                                              "(d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue)")]
    [TestCase("(d, e) => OnValueChanged(d, e)",                              "(d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue)")]
    [TestCase("new PropertyChangedCallback(OnValueChanged)",                 "new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue))")]
    [TestCase("new PropertyChangedCallback((d, e) => OnValueChanged(d, e))", "new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue))")]
    public static void DependencyPropertyRegisterPropertyChangedCallback(string call, string lambda)
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
                ↓OnValueChanged));

        public string Value
        {
            get => (string)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void OnValueChanged(string oldValue, string newValue)
        {
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue);
        }
    }
}".AssertReplace("↓OnValueChanged", $"↓{call}");

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
                (d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue)));

        public string Value
        {
            get => (string)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void OnValueChanged(string oldValue, string newValue)
        {
        }
    }
}".AssertReplace("(d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue)", lambda);

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, after);
    }

    [Test]
    public static void DependencyPropertyRegisterPropertyChangedCallbackFrameworkPropertyMetadata()
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
            new FrameworkPropertyMetadata(
                default(string),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                ↓OnValueChanged));

        public string? Value
        {
            get => (string?)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void OnValueChanged(string? oldValue, string? newValue)
        {
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FooControl)d).OnValueChanged((string?)e.OldValue, (string?)e.NewValue);
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
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(string),
            typeof(FooControl),
            new FrameworkPropertyMetadata(
                default(string),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) => ((FooControl)d).OnValueChanged((string?)e.OldValue, (string?)e.NewValue)));

        public string? Value
        {
            get => (string?)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void OnValueChanged(string? oldValue, string? newValue)
        {
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void Issue285()
    {
        var part1 = @"
namespace N
{
    using System;
    using System.Windows.Controls;

    public partial class MediaElementWrapper : Decorator
    {
        protected virtual void OnSourceChanged(Uri? source)
        {
        }
    }
}";
        var before = @"
namespace N
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public partial class MediaElementWrapper
    {
        /// <summary>
        /// Identifies the <see cref=""MediaElementWrapper.Source"" /> dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the <see cref=""MediaElementWrapper.Source"" /> dependency property.
        /// </returns>
        public static readonly DependencyProperty SourceProperty = MediaElement.SourceProperty.AddOwner(
            typeof(MediaElementWrapper),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                ↓OnSourceChanged,
                OnSourceCoerce));

        /// <summary>
        /// Gets or sets a media source on the <see cref=""MediaElementWrapper"" />.
        /// </summary>
        /// <returns>
        /// The URI that specifies the source of the element. The default is null.
        /// </returns>
        public Uri? Source
        {
            get => (Uri?)this.GetValue(SourceProperty);
            set => this.SetValue(SourceProperty, value);
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MediaElementWrapper)d).OnSourceChanged(e.NewValue as Uri);
        }

        private static object? OnSourceCoerce(DependencyObject d, object? baseValue)
        {
            var uri = baseValue as Uri;
            if (string.IsNullOrWhiteSpace(uri?.OriginalString))
            {
                return null;
            }

            return baseValue;
        }
    }
}";

        var after = @"
namespace N
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public partial class MediaElementWrapper
    {
        /// <summary>
        /// Identifies the <see cref=""MediaElementWrapper.Source"" /> dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the <see cref=""MediaElementWrapper.Source"" /> dependency property.
        /// </returns>
        public static readonly DependencyProperty SourceProperty = MediaElement.SourceProperty.AddOwner(
            typeof(MediaElementWrapper),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) => ((MediaElementWrapper)d).OnSourceChanged(e.NewValue as Uri),
                OnSourceCoerce));

        /// <summary>
        /// Gets or sets a media source on the <see cref=""MediaElementWrapper"" />.
        /// </summary>
        /// <returns>
        /// The URI that specifies the source of the element. The default is null.
        /// </returns>
        public Uri? Source
        {
            get => (Uri?)this.GetValue(SourceProperty);
            set => this.SetValue(SourceProperty, value);
        }

        private static object? OnSourceCoerce(DependencyObject d, object? baseValue)
        {
            var uri = baseValue as Uri;
            if (string.IsNullOrWhiteSpace(uri?.OriginalString))
            {
                return null;
            }

            return baseValue;
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { part1, before }, new[] { part1, after });
    }

    [TestCase("↓OnValueChanged",                                             "(d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue)")]
    [TestCase("(d, e) => OnValueChanged(d, e)",                              "(d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue)")]
    [TestCase("new PropertyChangedCallback(OnValueChanged)",                 "new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue))")]
    [TestCase("new PropertyChangedCallback((d, e) => OnValueChanged(d, e))", "new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue))")]
    public static void InlineCastStatement(string call, string lambda)
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
                ↓OnValueChanged));

        public string Value
        {
            get => (string)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void OnValueChanged(string oldValue, string newValue)
        {
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooControl)d;
            control.OnValueChanged((string)e.OldValue, (string)e.NewValue);
        }
    }
}".AssertReplace("↓OnValueChanged", $"↓{call}");

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
                (d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue)));

        public string Value
        {
            get => (string)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void OnValueChanged(string oldValue, string newValue)
        {
        }
    }
}".AssertReplace("(d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue)", lambda);

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, after);
    }

    [Test]
    public static void CoerceValueCallback()
    {
        var code = @"
namespace N
{
    using System.Windows;

    public class Issue252 : FrameworkElement
    {
        /// <summary>Identifies the <see cref=""Text""/> dependency property.</summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(Issue252),
            new PropertyMetadata(
                default(string),
                (d, e) => { },
                ↓CoerceText));

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        private static object CoerceText(DependencyObject d, object baseValue)
        {
            return baseValue ?? new object();
        }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;

    public class Issue252 : FrameworkElement
    {
        /// <summary>Identifies the <see cref=""Text""/> dependency property.</summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(Issue252),
            new PropertyMetadata(
                default(string),
                (d, e) => { },
                (d, baseValue) => baseValue ?? new object()));

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, after);
    }

    [Test]
    public static void WhenBinaryExpression()
    {
        var code = @"
namespace N
{
    using System.Windows;

    public class C : FrameworkElement
    {
        /// <summary>Identifies the <see cref=""Text""/> dependency property.</summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(C),
            new PropertyMetadata(
                default(string),
                ↓(d, e) => SetHasText(d, e.NewValue is { })));

        private static readonly DependencyPropertyKey HasTextPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(HasText),
            typeof(bool),
            typeof(C),
            new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        public bool HasText
        {
            get => (bool)this.GetValue(HasTextProperty);
            private set => this.SetValue(HasTextPropertyKey, value);
        }

        private static void SetHasText(DependencyObject o, bool value)
        {
            o.SetValue(HasTextPropertyKey, value);
        }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;

    public class C : FrameworkElement
    {
        /// <summary>Identifies the <see cref=""Text""/> dependency property.</summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(C),
            new PropertyMetadata(
                default(string),
                (d, e) => d.SetValue(HasTextPropertyKey, e.NewValue is { })));

        private static readonly DependencyPropertyKey HasTextPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(HasText),
            typeof(bool),
            typeof(C),
            new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        public bool HasText
        {
            get => (bool)this.GetValue(HasTextProperty);
            private set => this.SetValue(HasTextPropertyKey, value);
        }
    }
}";

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, after);
    }
}