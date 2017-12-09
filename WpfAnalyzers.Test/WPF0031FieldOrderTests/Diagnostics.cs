namespace WpfAnalyzers.Test.WPF0031FieldOrderTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("WPF0031");

        [Test]
        public void DependencyProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        // referencing field initialize below
        ↓public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            protected set {  this.SetValue(BarPropertyKey, value); }
        }
    }
}";

            AnalyzerAssert.Diagnostics<DependencyPropertyBackingFieldOrPropertyAnalyzer>(ExpectedDiagnostic, testCode);
        }

        [Test]
        public void Attached()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        ↓public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetBar(DependencyObject element, int value)
        {
            element.SetValue(BarPropertyKey, value);
        }

        public static int GetBar(DependencyObject element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

            AnalyzerAssert.Diagnostics<DependencyPropertyBackingFieldOrPropertyAnalyzer>(ExpectedDiagnostic, testCode);
        }
    }
}