namespace WpfAnalyzers.Test.WPF0006CoerceValueCallbackShouldMatchRegisteredNameTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new PropertyMetadataAnalyzer();
        private static readonly CodeFixProvider Fix = new RenameMemberFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0006CoerceValueCallbackShouldMatchRegisteredName.Descriptor);

        [Test]
        public static void UsedByMoreThanOnePropertyMatchingNeither()
        {
            var testCode = @"
namespace RoslynSandbox
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
            new PropertyMetadata(default(int), Meh, CoerceBar));

        /// <summary>Identifies the <see cref=""Baz""/> dependency property.</summary>
        public static readonly DependencyProperty BazProperty = DependencyProperty.Register(
            nameof(Baz),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int), Meh, ↓CoerceBar));

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

        private static void Meh(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((int)e.NewValue > 0)
            {
                d.ClearValue(BackgroundProperty);
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
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
            RoslynAssert.NoFix(Analyzer, Fix, ExpectedDiagnostic, testCode);
        }
    }
}
