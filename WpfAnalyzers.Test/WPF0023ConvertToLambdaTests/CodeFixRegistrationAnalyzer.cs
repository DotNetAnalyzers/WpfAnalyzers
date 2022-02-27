namespace WpfAnalyzers.Test.WPF0023ConvertToLambdaTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class CodeFixRegistrationAnalyzer
{
    private static readonly RegistrationAnalyzer Analyzer = new();
    private static readonly ConvertToLambdaFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0023ConvertToLambda);

    [TestCase("ValidateValue",                                    "value => (int)value >= 0")]
    [TestCase("x => ValidateValue(x)",                            "x => (int)x >= 0")]
    [TestCase("new ValidateValueCallback(ValidateValue)",         "new ValidateValueCallback(value => (int)value >= 0)")]
    [TestCase("new ValidateValueCallback(x => ValidateValue(x))", "new ValidateValueCallback(x => (int)x >= 0)")]
    public static void RemoveMethod(string callback, string lambda)
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
            new PropertyMetadata(default(int)),
            ↓ValidateValue);

        public int Value
        {
            get => (int)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        private static bool ValidateValue(object value)
        {
            return (int)value >= 0;
        }
    }
}".AssertReplace("ValidateValue);", $"{callback});");

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
            new PropertyMetadata(default(int)),
            value => (int)value >= 0);

        public int Value
        {
            get => (int)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }
    }
}".AssertReplace("value => (int)value >= 0", lambda);

        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, fixTitle: "Convert to lambda");
    }
}
