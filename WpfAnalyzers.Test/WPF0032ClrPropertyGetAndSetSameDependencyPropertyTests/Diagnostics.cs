namespace WpfAnalyzers.Test.WPF0032ClrPropertyGetAndSetSameDependencyPropertyTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ClrPropertyDeclarationAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("WPF0032");

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

        ↓public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(OtherProperty, value); }
        }
    }
}";
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                "WPF0032",
                "Property 'Bar' must access same dependency property in getter and setter");
            AnalyzerAssert.Diagnostics(Analyzer, expectedDiagnostic, testCode);
        }

        [Test]
        public void DependencyPropertyExpressionBodyAccessors()
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

        ↓public int Bar
        {
            get => (int)this.GetValue(BarProperty);
            set => this.SetValue(OtherProperty, value);
        }
    }
}";
            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
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

        public static readonly DependencyProperty OtherProperty = DependencyProperty.Register(
            ""Other"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        ↓public int Bar
        {
            get { return (int)this.GetValue(OtherProperty); }
            set { this.SetValue(BarPropertyKey, value); }
        }
    }
}";
            var expectedDiagnostic = ExpectedDiagnostic.Create(
                "WPF0032",
                "Property 'Bar' must access same dependency property in getter and setter");
            AnalyzerAssert.Diagnostics(Analyzer, expectedDiagnostic, testCode);
        }
    }
}
