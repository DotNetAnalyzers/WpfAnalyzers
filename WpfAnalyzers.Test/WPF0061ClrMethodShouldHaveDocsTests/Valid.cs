namespace WpfAnalyzers.Test.WPF0061ClrMethodShouldHaveDocsTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly ClrMethodDeclarationAnalyzer Analyzer = new();

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

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void Multiline()
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

        /// <summary>
        /// Helper for setting <see cref=""BarProperty""/> on <paramref name=""element""/>.
        /// </summary>
        /// <param name=""element"">
        /// <see cref=""UIElement""/> to set <see cref=""BarProperty""/> on.
        /// </param>
        /// <param name=""value"">
        /// Bar property value.
        /// </param>
        public static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        /// <summary>
        /// Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.
        /// </summary>
        /// <param name=""element"">
        /// <see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.
        /// </param>
        /// <returns>
        /// Bar property value.
        /// </returns>
        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
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

    public class Foo
    {
        protected static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        /// <summary>Helper for getting <see cref=""BarProperty""/> from <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to read <see cref=""BarProperty""/> from.</param>
        /// <returns>Bar property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }

        /// <summary>Helper for setting <see cref=""BarPropertyKey""/> on <paramref name=""element""/>.</summary>
        /// <param name=""element""><see cref=""UIElement""/> to set <see cref=""BarPropertyKey""/> on.</param>
        /// <param name=""value"">Bar property value.</param>
        protected static void SetBar(UIElement element, int value)
        {
            element.SetValue(BarPropertyKey, value);
        }
    }
}";

        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void DependencyPropertyRegisterAttachedReadOnlyExpressionBodyExtensionMethods()
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
    public static void DependencyPropertyRegisterReadOnlyBackingFields()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;
    
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
    public static void DependencyPropertyRegisterReadOnlyBackingProperties()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static DependencyPropertyKey BarPropertyKey { get; } = DependencyProperty.RegisterReadOnly(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static DependencyProperty BarProperty { get; } = BarPropertyKey.DependencyProperty;
    
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
    public static void DependencyPropertyAddOwner()
    {
        var fooCode = @"
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
        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static int GetBar(UIElement element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static readonly DependencyProperty BarProperty = Foo.BarProperty.AddOwner(typeof(FooControl));

        public int Bar
        {
            get { return (int) this.GetValue(BarProperty); }
            set { this.SetValue(BarProperty, value); }
        }
    }
}";

        RoslynAssert.Valid(Analyzer, fooCode, code);
    }

    [Test]
    public static void DependencyPropertyRegisterBackingField()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(nameof(Bar), typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

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
    public static void DependencyPropertyRegisterBackingProperty()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static DependencyProperty BarProperty { get; } = DependencyProperty.Register(nameof(Bar), typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

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
    public static void DependencyPropertyRegisterFormatted()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
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
    public static void DependencyPropertyRegisterPartial()
    {
        var part1 = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class FooControl : Control
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));
    }
}";

        var part2 = @"
namespace N
{
    using System.Windows;

    public partial class FooControl
    {
        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarPropertyKey, value); }
        }
    }
}";

        RoslynAssert.Valid(Analyzer, part1, part2);
    }

    [Test]
    public static void FullyQualified()
    {
        var code = @"
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

        RoslynAssert.Valid(Analyzer, code);
    }
}