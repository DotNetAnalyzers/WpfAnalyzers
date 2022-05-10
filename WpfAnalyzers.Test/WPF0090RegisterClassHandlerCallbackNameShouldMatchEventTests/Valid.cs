namespace WpfAnalyzers.Test.WPF0090RegisterClassHandlerCallbackNameShouldMatchEventTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly RoutedEventCallbackAnalyzer Analyzer = new();

    [Test]
    public static void WhenCorrectNameSizeChangedEvent()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        static FooControl()
        {
            EventManager.RegisterClassHandler(typeof(TextBlock), SizeChangedEvent, new SizeChangedEventHandler(OnSizeChanged));
        }

        private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void WhenCorrectNameSizeChangedEventHandledEventsToo()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        static FooControl()
        {
            EventManager.RegisterClassHandler(typeof(TextBlock), SizeChangedEvent, new SizeChangedEventHandler(OnSizeChanged), true);
        }

        private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void WhenCorrectNameManipulationStartingEvent()
    {
        var code = @"
namespace N
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public class FooControl : Control
    {
        static FooControl()
        {
            EventManager.RegisterClassHandler(typeof(UIElement), ManipulationStartingEvent, new EventHandler<ManipulationStartingEventArgs>(OnManipulationStarting));
        }

        private static void OnManipulationStarting(object? sender, ManipulationStartingEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void WhenCorrectNameMouseDownEvent()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public class FooControl : Control
    {
        static FooControl()
        {
            EventManager.RegisterClassHandler(typeof(ComboBox), Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseDown), true);
        }

        private static void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void WhenUsedByTwoMouseButtonEventArgs()
    {
        var code = @"
namespace N;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public static class C
{
    static C()
    {
        EventManager.RegisterClassHandler(typeof(TreeViewItem), UIElement.MouseLeftButtonDownEvent,  new MouseButtonEventHandler(OnDown));
        EventManager.RegisterClassHandler(typeof(TreeViewItem), UIElement.MouseRightButtonDownEvent, new MouseButtonEventHandler(OnDown));

        static void OnDown(object sender, MouseButtonEventArgs e)
        {
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void WhenUsedByTwoEventArgs()
    {
        var code = @"
namespace N;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public static class C
{
    static C()
    {
        EventManager.RegisterClassHandler(typeof(DataGridRow), UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnDown));
        EventManager.RegisterClassHandler(typeof(DataGridRow), UIElement.TouchDownEvent, new EventHandler<TouchEventArgs>(OnDown));

        static void OnDown(object? sender, EventArgs e)
        {
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }
}
