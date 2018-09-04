namespace WpfAnalyzers.Test.WPF0091AddAndRemoveHandlerCallbackNameShouldMatchEventTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class ValidCode
    {
        private static readonly RoutedEventCallbackAnalyzer Analyzer = new RoutedEventCallbackAnalyzer();

        [Test]
        public void AddHandlerSizeChangedEvent()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public FooControl()
        {
            this.AddHandler(SizeChangedEvent, new RoutedEventHandler(OnSizeChanged));
        }

        private static void OnSizeChanged(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void RemoveHandlerSizeChangedEvent()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public FooControl()
        {
            this.AddHandler(SizeChangedEvent, new RoutedEventHandler(OnSizeChanged));
        }

        private static void OnSizeChanged(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void AddHandlerManipulationStartingEvent()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Input;

    public class FooControl : Control
    {
        public FooControl()
        {
            this.AddHandler(ManipulationStartingEvent, new EventHandler<ManipulationStartingEventArgs>(OnManipulationStarting));
        }

        private static void OnManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void RemoveHandlerManipulationStartingEvent()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Input;

    public class FooControl : Control
    {
        public FooControl()
        {
            this.RemoveHandler(ManipulationStartingEvent, new EventHandler<ManipulationStartingEventArgs>(OnManipulationStarting));
        }

        private static void OnManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void AddHandlerMouseDownEvent()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Controls;
    using System.Windows.Input;

    public class FooControl : Control
    {
        public FooControl()
        {
            this.AddHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseDown));
        }

        private static void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void RemoveHandlerMouseDownEvent()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Controls;
    using System.Windows.Input;

    public class FooControl : Control
    {
        public FooControl()
        {
            this.RemoveHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseDown));
        }

        private static void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void AddPreviewMouseDownHandlerNewHandler()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Controls;
    using System.Windows.Input;

    public class FooControl : Control
    {
        public FooControl()
        {
            Mouse.AddPreviewMouseDownHandler(this, new MouseButtonEventHandler(OnPreviewMouseDown));
        }

        private static void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void AddPreviewMouseDownHandlerMethodGroup()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Controls;
    using System.Windows.Input;

    public class FooControl : Control
    {
        public FooControl()
        {
            Mouse.AddPreviewMouseDownHandler(this, OnPreviewMouseDown);
        }

        private static void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}
