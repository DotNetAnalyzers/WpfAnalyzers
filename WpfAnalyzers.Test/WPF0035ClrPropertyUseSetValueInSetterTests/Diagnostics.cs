namespace WpfAnalyzers.Test.WPF0035ClrPropertyUseSetValueInSetterTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class Diagnostics
    {
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
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty OtherProperty = DependencyProperty.Register(
            ""Other"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { ↓this.SetCurrentValue(OtherProperty, value); }
        }
    }
}";
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                "WPF0035",
                "Use SetValue in setter.");
            AnalyzerAssert.Diagnostics<PropertyDeclarationAnalyzer>(expectedDiagnostic, testCode);
        }

        [Test]
        public void DependencyPropertyAndReadOnlyDependencyProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public int Bar
        {
            get { return (int)this.GetValue(OtherProperty); }
            set { ↓this.SetCurrentValue(BarPropertyKey, value); }
        }
    }
}";
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                "WPF0035",
                "Use SetValue in setter.");
            AnalyzerAssert.Diagnostics<PropertyDeclarationAnalyzer>(expectedDiagnostic, testCode);
        }
    }
}