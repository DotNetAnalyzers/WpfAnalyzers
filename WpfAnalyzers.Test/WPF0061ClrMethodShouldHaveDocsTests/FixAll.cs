namespace WpfAnalyzers.Test.WPF0061ClrMethodShouldHaveDocsTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class FixAll
{
    private static readonly ClrMethodDeclarationAnalyzer Analyzer = new();
    private static readonly DocumentationFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0061DocumentClrMethod);

    [Test]
    public static void DependencyPropertyRegisterAttachedBothMissingDocs()
    {
        var before = @"
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

        public static void ↓SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int ↓GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

        var after = @"
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

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DependencyPropertyRegisterAttachedBothMissingDocsDifferentTypeAndParameterName()
    {
        var before = @"
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

        public static void ↓SetBar(DependencyObject d, int i)
        {
            d.SetValue(BarProperty, i);
        }

        public static int ↓GetBar(DependencyObject d)
        {
            return (int)d.GetValue(BarProperty);
        }
    }
}";

        var after = @"
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

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""d""/>.</summary>
        /// <param name=""d""><see cref=""DependencyObject""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""i"">Bar property value.</param>
        public static void SetBar(DependencyObject d, int i)
        {
            d.SetValue(BarProperty, i);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""d""/>.</summary>
        /// <param name=""d""><see cref=""DependencyObject""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(DependencyObject d)
        {
            return (int)d.GetValue(BarProperty);
        }
    }
}";
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DependencyPropertyRegisterAttachedGetMethodMissingDocs()
    {
        var before = @"
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

        public static int ↓GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

        var after = @"
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

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DependencyPropertyRegisterAttachedSetMethodMissingDocs()
    {
        var before = @"
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

        public static void ↓SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }
    }
}";

        var after = @"
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

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }
    }
}";
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DependencyPropertyRegisterAttachedWithAttachedPropertyBrowsableForType()
    {
        var before = @"
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

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static int ↓GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

        var after = @"
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

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DependencyPropertyRegisterAttachedNotStandardTextGetMethod()
    {
        var before = @"
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

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>↓Helper for getting Wrong property from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

        var after = @"
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

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DependencyPropertyRegisterAttachedNotStandardTextSetMethod()
    {
        var before = @"
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

        /// <summary>Helper for setting <see cref=""↓Wrong""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

        var after = @"
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

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DependencyPropertyRegisterAttachedReadOnlyExpressionBodyExtensionMethods()
    {
        var before = @"
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

        public static void ↓SetBar(this FrameworkElement element, int value) => element.SetValue(BarPropertyKey, value);

        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static int ↓GetBar(this FrameworkElement element) => (int)element.GetValue(BarProperty);
    }
}";

        var after = @"
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
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DependencyPropertyAddOwner()
    {
        var before = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int), 
            typeof(Foo), 
            new FrameworkPropertyMetadata(
                default(int), 
                FrameworkPropertyMetadataOptions.Inherits));

        public static void ↓SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int ↓GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

        var after = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            ""Bar"",
            typeof(int), 
            typeof(Foo), 
            new FrameworkPropertyMetadata(
                default(int), 
                FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void FullyQualified()
    {
        var before = @"
namespace ValidCode.AttachedProperties
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public static class ScrollViewer
    {
        public static readonly DependencyProperty AutoScrollToBottomProperty = DependencyProperty.RegisterAttached(
            ""AutoScrollToBottom"",
            typeof(bool),
            typeof(ScrollViewer),
            new PropertyMetadata(
                false,
                OnAutoScrollToBottomChanged));

        [AttachedPropertyBrowsableForType(typeof(System.Windows.Controls.ScrollViewer))]
        public static bool ↓GetAutoScrollToBottom(System.Windows.Controls.ScrollViewer element) => (bool)element.GetValue(AutoScrollToBottomProperty);

        public static void ↓SetAutoScrollToBottom(System.Windows.Controls.ScrollViewer element, bool value) => element.SetValue(AutoScrollToBottomProperty, value);

        private static void OnAutoScrollToBottomChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ScrollViewer scrollViewer &&
                e.NewValue is bool value)
            {
                if (value)
                {
                    scrollViewer.AutoScrollToBottom();
                    scrollViewer.ScrollChanged += ScrollChanged;
                }
                else
                {
                    scrollViewer.ScrollChanged -= ScrollChanged;
                }
            }
        }

        private static void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ScrollViewer scrollViewer &&
                e.ExtentHeightChange > 0)
            {
                scrollViewer.AutoScrollToBottom();
            }
        }

        private static void AutoScrollToBottom(this System.Windows.Controls.ScrollViewer scrollViewer)
        {
            if (Math.Abs(scrollViewer.VerticalOffset + scrollViewer.ActualHeight - scrollViewer.ExtentHeight) < 1)
            {
                scrollViewer.ScrollToBottom();
            }
        }
    }
}";

        var after = @"
namespace ValidCode.AttachedProperties
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public static class ScrollViewer
    {
        public static readonly DependencyProperty AutoScrollToBottomProperty = DependencyProperty.RegisterAttached(
            ""AutoScrollToBottom"",
            typeof(bool),
            typeof(ScrollViewer),
            new PropertyMetadata(
                false,
                OnAutoScrollToBottomChanged));

        /// <summary>Helper for getting <see cref=""AutoScrollToBottomProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""System.Windows.Controls.ScrollViewer""/> to read <see cref=""AutoScrollToBottomProperty""/> from.</param>
        /// <returns>AutoScrollToBottom property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(System.Windows.Controls.ScrollViewer))]
        public static bool GetAutoScrollToBottom(System.Windows.Controls.ScrollViewer element) => (bool)element.GetValue(AutoScrollToBottomProperty);

        /// <summary>Helper for setting <see cref=""AutoScrollToBottomProperty""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""System.Windows.Controls.ScrollViewer""/> to set <see cref=""AutoScrollToBottomProperty""/> on.</param>
        /// <param name=""value"">AutoScrollToBottom property value.</param>
        public static void SetAutoScrollToBottom(System.Windows.Controls.ScrollViewer element, bool value) => element.SetValue(AutoScrollToBottomProperty, value);

        private static void OnAutoScrollToBottomChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ScrollViewer scrollViewer &&
                e.NewValue is bool value)
            {
                if (value)
                {
                    scrollViewer.AutoScrollToBottom();
                    scrollViewer.ScrollChanged += ScrollChanged;
                }
                else
                {
                    scrollViewer.ScrollChanged -= ScrollChanged;
                }
            }
        }

        private static void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ScrollViewer scrollViewer &&
                e.ExtentHeightChange > 0)
            {
                scrollViewer.AutoScrollToBottom();
            }
        }

        private static void AutoScrollToBottom(this System.Windows.Controls.ScrollViewer scrollViewer)
        {
            if (Math.Abs(scrollViewer.VerticalOffset + scrollViewer.ActualHeight - scrollViewer.ExtentHeight) < 1)
            {
                scrollViewer.ScrollToBottom();
            }
        }
    }
}";

        RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }
}
