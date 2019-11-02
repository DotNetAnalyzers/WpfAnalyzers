namespace WpfAnalyzers.Test.WPF0042AvoidSideEffectsInClrAccessorsTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly ClrMethodDeclarationAnalyzer Analyzer = new ClrMethodDeclarationAnalyzer();

        [Test]
        public static void DependencyProperty()
        {
            var code = @"
namespace N
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

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void DependencyPropertyExpressionBodyAccessors()
        {
            var code = @"
namespace N
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

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void DependencyPropertyWithThis()
        {
            var code = @"
namespace N
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

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void ReadOnlyDependencyProperty()
        {
            var code = @"
namespace N
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

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void DependencyPropertyRegisterAttached()
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

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void DependencyPropertyRegisterAttachedExpressionBodies()
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

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void DependencyPropertyRegisterAttachedReadOnly()
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

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void DependencyPropertyRegisterAttachedReadOnlyExpressionBodies()
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

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void AttachedPropertyWithNullCheckBlocks()
        {
            var code = @"
namespace N
{
    using System;
    using System.Windows;

    public static class WithNullChecks
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            ""Text"",
            typeof(string),
            typeof(WithNullChecks),
            new PropertyMetadata(default(string)));

        /// <summary>Helper for setting <see cref=""TextProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to set <see cref=""TextProperty""/> on.</param>
        /// <param name=""value"">Text property value.</param>
        public static void SetText(DependencyObject element, string value)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            element.SetValue(TextProperty, value);
        }

        /// <summary>Helper for getting <see cref=""TextProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to read <see cref=""TextProperty""/> from.</param>
        /// <returns>Text property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static string GetText(DependencyObject element)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return (string)element.GetValue(TextProperty);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void AttachedPropertyWithNullChecks()
        {
            var code = @"
namespace N
{
    using System;
    using System.Windows;

    public static class WithNullChecks
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            ""Text"",
            typeof(string),
            typeof(WithNullChecks),
            new PropertyMetadata(default(string)));

        /// <summary>Helper for setting <see cref=""TextProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to set <see cref=""TextProperty""/> on.</param>
        /// <param name=""value"">Text property value.</param>
        public static void SetText(DependencyObject element, string value)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));

            element.SetValue(TextProperty, value);
        }

        /// <summary>Helper for getting <see cref=""TextProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to read <see cref=""TextProperty""/> from.</param>
        /// <returns>Text property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static string GetText(DependencyObject element)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));

            return (string)element.GetValue(TextProperty);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void AttachedPropertyWithEmptyIfs()
        {
            var code = @"
namespace N
{
    using System;
    using System.Windows;

    public static class WithNullChecks
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            ""Text"",
            typeof(string),
            typeof(WithNullChecks),
            new PropertyMetadata(default(string)));

        /// <summary>Helper for setting <see cref=""TextProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to set <see cref=""TextProperty""/> on.</param>
        /// <param name=""value"">Text property value.</param>
        public static void SetText(DependencyObject element, string value)
        {
            if (element is null)
            {
                // not typed the throw yet
            }

            element.SetValue(TextProperty, value);
        }

        /// <summary>Helper for getting <see cref=""TextProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""DependencyObject""/> to read <see cref=""TextProperty""/> from.</param>
        /// <returns>Text property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static string GetText(DependencyObject element)
        {
            if (element is null)
            {
                // not typed the throw yet
            }

            return (string)element.GetValue(TextProperty);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
