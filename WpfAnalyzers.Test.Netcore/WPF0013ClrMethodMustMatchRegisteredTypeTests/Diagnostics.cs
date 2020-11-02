namespace WpfAnalyzers.Test.Netcore.WPF0013ClrMethodMustMatchRegisteredTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ClrMethodDeclarationAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0013ClrMethodMustMatchRegisteredType);

        [Test]
        public static void Message()
        {
            var code = @"
#nullable enable
namespace N
{
    using System.Windows;

    public static class Foo
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(string),
            typeof(Foo),
            new PropertyMetadata(
                default(string),
                (o, e) => { }));

            public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public static void SetBar(this FrameworkElement element, ↓string value) => element.SetValue(BarPropertyKey, value);

        public static string? GetBar(this FrameworkElement element) => (string?)element.GetValue(BarProperty);
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Value type must match registered type string?"), code);
        }

        [TestCase("default(string)")]
        [TestCase("null")]
        public static void GetMethod(string expression)
        {
            var code = @"
#nullable enable
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(string),
            typeof(Foo),
            new PropertyMetadata(
                default(string),
                (o, e) => { }));

        public static void SetBar(this FrameworkElement element, string? value)
        {
            element.SetValue(BarProperty, value);
        }

        public static ↓string GetBar(this FrameworkElement element)
        {
            return (string)element.GetValue(BarProperty);
        }
    }
}".AssertReplace("default(string)", expression);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("default(string)")]
        [TestCase("null")]
        [TestCase("(object)null")]
        public static void SetMethod(string expression)
        {
            var code = @"
#nullable enable
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(string),
            typeof(Foo),
            new PropertyMetadata(
                default(string),
                (o, e) => { }));

        public static void SetBar(this FrameworkElement element, ↓string value)
        {
            element.SetValue(BarProperty, value);
        }

        public static string? GetBar(this FrameworkElement element)
        {
            return (string?)element.GetValue(BarProperty);
        }
    }
}".AssertReplace("default(string)", expression);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void ImplicitPropertyMetadata()
        {
            var code = @"
#nullable enable
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(string),
            typeof(Foo));

        public static void SetBar(this FrameworkElement element, ↓string value)
        {
            element.SetValue(BarProperty, value);
        }

        public static string? GetBar(this FrameworkElement element)
        {
            return (string?)element.GetValue(BarProperty);
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
