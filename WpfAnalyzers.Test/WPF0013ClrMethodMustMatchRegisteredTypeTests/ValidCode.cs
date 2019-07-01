namespace WpfAnalyzers.Test.WPF0013ClrMethodMustMatchRegisteredTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class ValidCode
    {
        private static readonly ClrMethodDeclarationAnalyzer Analyzer = new ClrMethodDeclarationAnalyzer();

        [TestCase("int")]
        [TestCase("int?")]
        [TestCase("int[]")]
        [TestCase("int?[]")]
        [TestCase("Nullable<int>")]
        [TestCase("ObservableCollection<int>")]
        public static void DependencyPropertyRegisterAttached(string typeName)
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

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""FrameworkElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(FrameworkElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""FrameworkElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static int GetBar(FrameworkElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}".AssertReplace("int", typeName);

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [TestCase("int")]
        [TestCase("int?")]
        [TestCase("int[]")]
        [TestCase("int?[]")]
        [TestCase("Nullable<int>")]
        [TestCase("ObservableCollection<int>")]
        public static void DependencyPropertyRegisterAttachedExtensionMethods(string typeName)
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

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""FrameworkElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
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
            return (int)element.GetValue(BarProperty);
        }
    }
}".AssertReplace("int", typeName);

            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void DependencyPropertyRegisterAttachedWhenBoxed()
        {
            var booleanBoxesCode = @"
namespace RoslynSandbox
{
    internal static class BooleanBoxes
    {
        internal static readonly object True = true;
        internal static readonly object False = false;

        internal static object Box(bool value)
        {
            return value
                        ? True
                        : False;
        }
    }
}";

            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(bool),
            typeof(Foo),
            new PropertyMetadata(default(bool)));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""FrameworkElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(FrameworkElement element, bool value)
        {
            element.SetValue(BarProperty, BooleanBoxes.Box(value));
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""FrameworkElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static bool GetBar(FrameworkElement element)
        {
            return (bool)element.GetValue(BarProperty);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, booleanBoxesCode, testCode);
        }

        [Test]
        public static void DependencyPropertyRegisterAttachedSettingValueInCallback()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
            ""Value"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(
                default(int),
                OnValueChanged));

        /// <summary>Helper for setting <see cref=""ValueProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to set <see cref=""ValueProperty""/> on.</param>
        /// <param name=""value"">Value property value.</param>
        public static void SetValue(this DependencyObject element, int value)
        {
            element.SetValue(ValueProperty, value);
        }

        /// <summary>Helper for getting <see cref=""ValueProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to read <see cref=""ValueProperty""/> from.</param>
        /// <returns>Value property value.</returns>
        [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static int GetValue(this DependencyObject element)
        {
            return (int)element.GetValue(ValueProperty);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(ValueProperty, e.NewValue);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void DependencyPropertyRegisterAttachedReadOnly()
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

            RoslynAssert.Valid(Analyzer, testCode);
        }
    }
}
