namespace WpfAnalyzers.Test.WPF0005PropertyChangedCallbackShouldMatchRegisteredNameTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Diagnostics
{
    private static readonly PropertyMetadataAnalyzer Analyzer = new();
    private static readonly RenameMemberFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0005PropertyChangedCallbackShouldMatchRegisteredName);

    [Test]
    public static void UsedByMoreThanOnePropertyMatchingNeither()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int), OnBarChanged));

        /// <summary>Identifies the <see cref=""Baz""/> dependency property.</summary>
        public static readonly DependencyProperty BazProperty = DependencyProperty.Register(
            nameof(Baz),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int), ↓OnBarChanged));

        public int Bar
        {
            get => (int)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }

        public int Baz
        {
            get => (int)this.GetValue(BazProperty);
            set => this.SetValue(BazProperty, value);
        }

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((int)e.NewValue > 0)
            {
                d.ClearValue(BackgroundProperty);
            }
        }
    }
}";
        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, code);
    }
}