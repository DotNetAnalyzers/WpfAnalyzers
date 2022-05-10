namespace WpfAnalyzers.Test.WPF0091AddAndRemoveHandlerCallbackNameShouldMatchEventTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class CodeFix
{
    private static readonly RoutedEventCallbackAnalyzer Analyzer = new();
    private static readonly RenameMemberFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0091CallbackNameShouldMatchEvent);

    [Test]
    public static void MessageAddHandler()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public FooControl()
        {
            this.AddHandler(SizeChangedEvent, new RoutedEventHandler(↓WrongName));
        }

        private static void WrongName(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Rename to OnSizeChanged to match the event"), code);
    }

    [Test]
    public static void MessageRemoveHandler()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public FooControl()
        {
            this.RemoveHandler(SizeChangedEvent, new RoutedEventHandler(↓WrongName));
        }

        private static void WrongName(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Rename to OnSizeChanged to match the event"), code);
    }

    [TestCase("new RoutedEventHandler(↓WrongName)", "new RoutedEventHandler(OnSizeChanged)")]
    [TestCase("new RoutedEventHandler((o, e) => ↓WrongName(o, e))", "new RoutedEventHandler((o, e) => OnSizeChanged(o, e))")]
    public static void WhenCorrectNameAddHandlerSizeChangedEvent(string beforeExpression, string afterExpression)
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public FooControl()
        {
            this.AddHandler(SizeChangedEvent, new RoutedEventHandler(↓WrongName));
        }

        private static void WrongName(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}".AssertReplace("new RoutedEventHandler(↓WrongName)", beforeExpression);

        var after = @"
namespace N
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
}".AssertReplace("new RoutedEventHandler(OnSizeChanged)", afterExpression);
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [TestCase("new RoutedEventHandler(↓WrongName)",                 "new RoutedEventHandler(OnSizeChanged)")]
    [TestCase("new RoutedEventHandler((o, e) => ↓WrongName(o, e))", "new RoutedEventHandler((o, e) => OnSizeChanged(o, e))")]
    public static void WhenCorrectNameRemoveHandlerSizeChangedEvent(string beforeExpression, string afterExpression)
    {
        var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public FooControl()
        {
            this.AddHandler(SizeChangedEvent, new RoutedEventHandler(↓WrongName));
        }

        private static void WrongName(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}".AssertReplace("new RoutedEventHandler(↓WrongName)", beforeExpression);

        var after = @"
namespace N
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
}".AssertReplace("new RoutedEventHandler(OnSizeChanged)", afterExpression);
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenCorrectNameAddHandlerManipulationStartingEvent()
    {
        var before = @"
namespace N
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Input;

    public class FooControl : Control
    {
        public FooControl()
        {
            this.AddHandler(ManipulationStartingEvent, new EventHandler<ManipulationStartingEventArgs>(↓WrongName));
        }

        private static void WrongName(object? sender, ManipulationStartingEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";

        var after = @"
namespace N
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

        private static void OnManipulationStarting(object? sender, ManipulationStartingEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenCorrectNameRemoveHandlerManipulationStartingEvent()
    {
        var before = @"
namespace N
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Input;

    public class FooControl : Control
    {
        public FooControl()
        {
            this.RemoveHandler(ManipulationStartingEvent, new EventHandler<ManipulationStartingEventArgs>(↓WrongName));
        }

        private static void WrongName(object? sender, ManipulationStartingEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";

        var after = @"
namespace N
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

        private static void OnManipulationStarting(object? sender, ManipulationStartingEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenCorrectNameAddHandlerMouseDownEvent()
    {
        var before = @"
namespace N
{
    using System.Windows.Controls;
    using System.Windows.Input;

    public class FooControl : Control
    {
        public FooControl()
        {
            this.AddHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(↓WrongName));
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
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenCorrectNameRemoveHandlerMouseDownEvent()
    {
        var before = @"
namespace N
{
    using System.Windows.Controls;
    using System.Windows.Input;

    public class FooControl : Control
    {
        public FooControl()
        {
            this.RemoveHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(↓WrongName));
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
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }
}
