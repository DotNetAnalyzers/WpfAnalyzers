namespace WpfAnalyzers.Test.WPF0013ClrMethodMustMatchRegisteredTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly ClrMethodDeclarationAnalyzer Analyzer = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0013ClrMethodMustMatchRegisteredType);

        [Test]
        public static void Message()
        {
            var code = @"
namespace N
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

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Value type must match registered type int"), code);
        }

        [TestCase("double")]
        [TestCase("int?")]
        [TestCase("Nullable<int>")]
        [TestCase("ObservableCollection<int>")]
        public static void DependencyPropertyRegisterAttachedSetMethod(string typeName)
        {
            var code = @"
#pragma warning disable CS8019
#nullable disable
namespace N
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

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void DependencyPropertyRegisterAttachedSetMethodAsExtensionMethod()
        {
            var code = @"
namespace N
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

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void DependencyPropertyRegisterAttachedReadOnlySetMethod()
        {
            var code = @"
namespace N
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

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [TestCase("double")]
        [TestCase("int?")]
        [TestCase("Nullable<int>")]
        [TestCase("ObservableCollection<int>")]
        public static void DependencyPropertyRegisterAttachedGetMethod(string typeName)
        {
            var code = @"
#pragma warning disable CS8019
#nullable disable
namespace N
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

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void DependencyPropertyRegisterAttachedGetMethodAsExtensionMethod()
        {
            var code = @"
namespace N
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

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
