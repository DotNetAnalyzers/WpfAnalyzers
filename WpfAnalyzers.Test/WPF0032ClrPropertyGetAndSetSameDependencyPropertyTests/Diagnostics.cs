namespace WpfAnalyzers.Test.WPF0032ClrPropertyGetAndSetSameDependencyPropertyTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ClrPropertyDeclarationAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0032ClrPropertyGetAndSetSameDependencyProperty.Descriptor);

        [Test]
        public static void DependencyProperty()
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

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Property 'Bar' must access same dependency property in getter and setter"), testCode);
        }

        [Test]
        public static void DependencyPropertyExpressionBodyAccessors()
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
            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [Test]
        public static void DependencyPropertyAndReadOnlyDependencyProperty()
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

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Property 'Bar' must access same dependency property in getter and setter"), testCode);
        }
    }
}
