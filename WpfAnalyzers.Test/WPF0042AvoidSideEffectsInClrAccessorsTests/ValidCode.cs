namespace WpfAnalyzers.Test.WPF0042AvoidSideEffectsInClrAccessorsTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly ClrMethodDeclarationAnalyzer Analyzer = new ClrMethodDeclarationAnalyzer();

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
            ""Bar"", 
            typeof(int), 
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
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
            ""Bar"", 
            typeof(int), 
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get => (int)GetValue(BarProperty);
            set => SetValue(BarProperty, value);
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void DependencyPropertyWithThis()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"", 
            typeof(int), 
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void ReadOnlyDependencyProperty()
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
            get { return (int)this.GetValue(BarProperty); }
            protected set { this.SetValue(BarPropertyKey, value); }
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void DependencyPropertyRegisterAttached()
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

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""FrameworkElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(this FrameworkElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""FrameworkElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static int GetBar(this FrameworkElement element)
        {
            return (int) element.GetValue(BarProperty);
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void DependencyPropertyRegisterAttachedExpressionBodies()
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

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""FrameworkElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarProperty, value);

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""FrameworkElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void DependencyPropertyRegisterAttachedReadOnly()
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

        /// <summary>Helper for setting <see cref=""BarPropertyKey""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""FrameworkElement""/> to set <see cref=""BarPropertyKey""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(this FrameworkElement element, int value)
        {
            element.SetValue(BarPropertyKey, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""FrameworkElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static int GetBar(this FrameworkElement element)
        {
            return (int) element.GetValue(BarProperty);
        }
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void DependencyPropertyRegisterAttachedReadOnlyExpressionBodies()
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

        /// <summary>Helper for setting <see cref=""BarPropertyKey""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""FrameworkElement""/> to set <see cref=""BarPropertyKey""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""FrameworkElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static int GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);
    }
}";

            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}
