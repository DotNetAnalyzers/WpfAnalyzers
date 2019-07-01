namespace WpfAnalyzers.Test.WPF0013ClrMethodMustMatchRegisteredTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ClrMethodDeclarationAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0013ClrMethodMustMatchRegisteredType.Descriptor);

        [Test]
        public static void Message()
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

        public static void SetBar(this FrameworkElement element, ↓double value) => element.SetValue(BarPropertyKey, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Value type must match registered type int"), testCode);
        }

        [TestCase("double")]
        [TestCase("int?")]
        [TestCase("Nullable<int>")]
        [TestCase("ObservableCollection<int>")]
        public static void DependencyPropertyRegisterAttachedSetMethod(string typeName)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetBar(FrameworkElement element, ↓double value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int GetBar(FrameworkElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}".AssertReplace("double", typeName);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [Test]
        public static void DependencyPropertyRegisterAttachedSetMethodAsExtensionMethod()
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
            new PropertyMetadata(default(int)));

        public static void SetBar(this FrameworkElement element, ↓double value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int GetBar(this FrameworkElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [Test]
        public static void DependencyPropertyRegisterAttachedReadOnlySetMethod()
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

        public static void SetBar(this FrameworkElement element, ↓double value) => element.SetValue(BarPropertyKey, value);

        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [TestCase("double")]
        [TestCase("int?")]
        [TestCase("Nullable<int>")]
        [TestCase("ObservableCollection<int>")]
        public static void DependencyPropertyRegisterAttachedGetMethod(string typeName)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static void SetBar(FrameworkElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static ↓double GetBar(FrameworkElement element)
        {
            return (double)element.GetValue(BarProperty);
        }
    }
}".AssertReplace("double", typeName);

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [Test]
        public static void DependencyPropertyRegisterAttachedGetMethodAsExtensionMethod()
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
            new PropertyMetadata(default(int)));

        public static void SetBar(this FrameworkElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static ↓double GetBar(this FrameworkElement element)
        {
            return (double)element.GetValue(BarProperty);
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }
    }
}
