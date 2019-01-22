namespace WpfAnalyzers.Test.WPF0042AvoidSideEffectsInClrAccessorsTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ClrMethodDeclarationAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0042AvoidSideEffectsInClrAccessors.Descriptor);

        [Test]
        public void Message()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(1));

        public static void SetBar(this FrameworkElement element, int value)
        {
            ↓SideEffect(); 
            element.SetValue(BarProperty, value);
        }

        public static int GetBar(this FrameworkElement element)
        {
            return (int) element.GetValue(BarProperty);
        }

        private static void SideEffect()
        {
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Avoid side effects in CLR accessors."), testCode);
        }

        [Test]
        public void DependencyPropertyRegisterAttachedWithSideEffectInSetMethod()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(1));

        public static void SetBar(this FrameworkElement element, int value)
        {
            ↓SideEffect(); 
            element.SetValue(BarProperty, value);
        }

        public static int GetBar(this FrameworkElement element)
        {
            return (int) element.GetValue(BarProperty);
        }

        private static void SideEffect()
        {
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [Test]
        public void DependencyPropertyRegisterAttachedWithSideEffectInGetMethod()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(1));

        public static void SetBar(this FrameworkElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int GetBar(this FrameworkElement element)
        {
            ↓SideEffect(); 
            return (int) element.GetValue(BarProperty);
        }

        private static void SideEffect()
        {
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [Test]
        public void DependencyPropertyRegisterAttachedReadOnlyWithSideEffectInSetMethod()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public static void SetBar(this FrameworkElement element, int value)
        {
            ↓SideEffect(); 
            element.SetValue(BarPropertyKey, value);
        }

        public static int GetBar(this FrameworkElement element)
        {
            return (int) element.GetValue(BarProperty);
        }

        private static void SideEffect()
        {
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [Test]
        public void DependencyPropertyRegisterAttachedReadOnlyWithSideEffectInGetMethod()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;


        public static void SetBar(this FrameworkElement element, int value)
        {
            element.SetValue(BarPropertyKey, value);
        }

        public static int GetBar(this FrameworkElement element)
        {
            ↓SideEffect(); 
            return (int) element.GetValue(BarProperty);
        }

        private static void SideEffect()
        {
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }
    }
}
