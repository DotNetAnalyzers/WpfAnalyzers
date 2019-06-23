namespace WpfAnalyzers.Test.WPF0091AddAndRemoveHandlerCallbackNameShouldMatchEventTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new RoutedEventCallbackAnalyzer();
        private static readonly CodeFixProvider Fix = new RenameMemberFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0091AddAndRemoveHandlerCallbackNameShouldMatchEvent.Descriptor);

        [Test]
        public void MessageAddHandler()
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
            this.AddHandler(SizeChangedEvent, new RoutedEventHandler(WrongName));
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
        public void MessageRemoveHandler()
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
            this.RemoveHandler(SizeChangedEvent, new RoutedEventHandler(WrongName));
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
        public void WhenCorrectNameAddHandlerSizeChangedEvent()
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
            this.AddHandler(SizeChangedEvent, new RoutedEventHandler(WrongName));
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenCorrectNameRemoveHandlerSizeChangedEvent()
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
            this.AddHandler(SizeChangedEvent, new RoutedEventHandler(WrongName));
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenCorrectNameAddHandlerManipulationStartingEvent()
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
            this.AddHandler(ManipulationStartingEvent, new EventHandler<ManipulationStartingEventArgs>(WrongName));
        }

        private static void WrongName(object sender, ManipulationStartingEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";

            var fixedCode = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenCorrectNameRemoveHandlerManipulationStartingEvent()
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
            this.RemoveHandler(ManipulationStartingEvent, new EventHandler<ManipulationStartingEventArgs>(WrongName));
        }

        private static void WrongName(object sender, ManipulationStartingEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";

            var fixedCode = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenCorrectNameAddHandlerMouseDownEvent()
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
            this.AddHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(WrongName));
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenCorrectNameRemoveHandlerMouseDownEvent()
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
            this.RemoveHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(WrongName));
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}
