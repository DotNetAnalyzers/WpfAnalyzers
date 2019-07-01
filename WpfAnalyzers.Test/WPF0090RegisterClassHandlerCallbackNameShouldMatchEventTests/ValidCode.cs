namespace WpfAnalyzers.Test.WPF0090RegisterClassHandlerCallbackNameShouldMatchEventTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class ValidCode
    {
        private static readonly RoutedEventCallbackAnalyzer Analyzer = new RoutedEventCallbackAnalyzer();

        [Test]
        public static void WhenCorrectNameSizeChangedEvent()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        static FooControl()
        {
            EventManager.RegisterClassHandler(typeof(TextBlock), SizeChangedEvent, new RoutedEventHandler(OnSizeChanged));
        }

        private static void OnSizeChanged(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void WhenCorrectNameSizeChangedEventHandledEventsToo()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        static FooControl()
        {
            EventManager.RegisterClassHandler(typeof(TextBlock), SizeChangedEvent, new RoutedEventHandler(OnSizeChanged), true);
        }

        private static void OnSizeChanged(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void WhenCorrectNameManipulationStartingEvent()
        {
            var testCode = @"
namespace RoslynSandbox
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

        private static void OnManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void WhenCorrectNameMouseDownEvent()
        {
            var testCode = @"
namespace RoslynSandbox
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
            RoslynAssert.Valid(Analyzer, testCode);
        }
    }
}
