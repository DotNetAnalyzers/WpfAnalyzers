namespace WpfAnalyzers.Test.Netcore.WPF0012ClrPropertyShouldMatchRegisteredTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ClrPropertyDeclarationAnalyzer();
        private static readonly CodeFixProvider Fix = new UseRegisteredTypeFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0012ClrPropertyShouldMatchRegisteredType);

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
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar), 
            typeof(string), 
            typeof(FooControl),
            new PropertyMetadata(default(string)));

        public ↓string Bar
        {
            get => (string)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Property 'N.FooControl.Bar' must be of type string?"), code);
        }

        [TestCase("default(string)")]
        public static void DependencyProperty(string expression)
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(string),
            typeof(FooControl),
            new PropertyMetadata(default(string)));

        public ↓string Bar
        {
            get => (string)GetValue(BarProperty);
            set => SetValue(BarProperty, value);
        }
    }
}".AssertReplace("default(string)", expression);

            var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(string),
            typeof(FooControl),
            new PropertyMetadata(default(string)));

        public string? Bar
        {
            get => (string?)GetValue(BarProperty);
            set => SetValue(BarProperty, value);
        }
    }
}".AssertReplace("default(string)", expression);

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
