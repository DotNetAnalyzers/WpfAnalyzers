namespace WpfAnalyzers.Test.WPF0090UseMatchingNameWithRegisterClassHandlerTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class CodeFix
    {
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("WPF0090");

        [Test]
        public void Message()
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
            EventManager.RegisterClassHandler(typeof(TextBlock), SizeChangedEvent, new RoutedEventHandler(↓WrongName));
        }

        private static void WrongName(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";

            var expectedDiagnostic = ExpectedDiagnostic.Create("WPF0090", "Rename to OnSizeChanged to match the event.");
            AnalyzerAssert.Diagnostics<CallbackNameShouldMatchEvent>(expectedDiagnostic, testCode);
        }

        [Test]
        public void WhenWrongNameSizeChangedEvent()
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
            EventManager.RegisterClassHandler(typeof(TextBlock), SizeChangedEvent, new RoutedEventHandler(↓WrongName));
        }

        private static void WrongName(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";

            var fixedCode = @"
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
            AnalyzerAssert.CodeFix<CallbackNameShouldMatchEvent, RenameMethodCodeFixProvider>(ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenWrongNameSizeChangedEventHandledEventsToo()
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
            EventManager.RegisterClassHandler(typeof(TextBlock), SizeChangedEvent, new RoutedEventHandler(↓WrongName), true);
        }

        private static void WrongName(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";

            var fixedCode = @"
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
            AnalyzerAssert.CodeFix<CallbackNameShouldMatchEvent, RenameMethodCodeFixProvider>(ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenCorrectNameMouseDownEvent()
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
            EventManager.RegisterClassHandler(typeof(ComboBox), Mouse.MouseDownEvent, new MouseButtonEventHandler(↓WrongName), true);
        }

        private static void WrongName(object sender, MouseButtonEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";

            var fixedCode = @"
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
            AnalyzerAssert.CodeFix<CallbackNameShouldMatchEvent, RenameMethodCodeFixProvider>(ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}