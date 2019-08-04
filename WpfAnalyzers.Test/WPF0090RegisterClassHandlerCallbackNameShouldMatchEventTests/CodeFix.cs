namespace WpfAnalyzers.Test.WPF0090RegisterClassHandlerCallbackNameShouldMatchEventTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new RoutedEventCallbackAnalyzer();
        private static readonly CodeFixProvider Fix = new RenameMemberFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0090RegisterClassHandlerCallbackNameShouldMatchEvent.Descriptor);

        [Test]
        public static void Message()
        {
            var testCode = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        static FooControl()
        {
            EventManager.RegisterClassHandler(typeof(TextBlock), SizeChangedEvent, new RoutedEventHandler(↓WrongName));
        }

        private static void WrongName(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Rename to OnSizeChanged to match the event."), testCode);
        }

        [Test]
        public static void WhenWrongNameSizeChangedEvent()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        static FooControl()
        {
            EventManager.RegisterClassHandler(typeof(TextBlock), SizeChangedEvent, new RoutedEventHandler(↓WrongName));
        }

        private static void WrongName(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";

            var after = @"
namespace N
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenWrongNameSizeChangedEventHandledEventsToo()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        static FooControl()
        {
            EventManager.RegisterClassHandler(typeof(TextBlock), SizeChangedEvent, new RoutedEventHandler(↓WrongName), true);
        }

        private static void WrongName(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";

            var after = @"
namespace N
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void WhenCorrectNameMouseDownEvent()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public class FooControl : Control
    {
        static FooControl()
        {
            EventManager.RegisterClassHandler(typeof(ComboBox), Mouse.MouseDownEvent, new MouseButtonEventHandler(↓WrongName), true);
        }

        private static void WrongName(object sender, MouseButtonEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";

            var after = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
