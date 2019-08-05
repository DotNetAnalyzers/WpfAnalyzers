namespace WpfAnalyzers.Test.WPF0023ConvertToLambdaTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new PropertyMetadataAnalyzer();
        private static readonly CodeFixProvider Fix = new ConvertToLambdaFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0023ConvertToLambda);

        [TestCase("new PropertyMetadata(default(string), OnValueChanged)", "(d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue)")]
        [TestCase("new PropertyMetadata(default(string), (d, e) => OnValueChanged(d, e))", "(d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue)")]
        [TestCase("new PropertyMetadata(default(string), new PropertyChangedCallback(OnValueChanged))", "new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue))")]
        [TestCase("new PropertyMetadata(default(string), new PropertyChangedCallback((d, e) => OnValueChanged(d, e)))", "new PropertyChangedCallback((d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue))")]
        public static void DependencyPropertyRegisterPropertyChangedCallback(string metadata, string callback)
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
            new PropertyMetadata(default(string), OnValueChanged));

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
            new PropertyMetadata(default(string), (d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue)));

        public string Value
        {
            get => (string)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        protected virtual void OnValueChanged(string oldValue, string newValue)
        {
        }
    }
}".AssertReplace("(d, e) => ((FooControl)d).OnValueChanged((string)e.OldValue, (string)e.NewValue)", callback);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, code, after);
        }
    }
}
