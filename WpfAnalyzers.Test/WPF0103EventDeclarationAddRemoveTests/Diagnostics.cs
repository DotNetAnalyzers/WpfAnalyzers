namespace WpfAnalyzers.Test.WPF0103EventDeclarationAddRemoveTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new RoutedEventEventDeclarationAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0103EventDeclarationAddRemove.Descriptor);

        [Test]
        public static void Message()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the Value1Changed event</summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            ""Value1Changed"",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(FooControl));

        /// <summary>Identifies the ValueChanged event</summary>
        public static readonly RoutedEvent Value2ChangedEvent = EventManager.RegisterRoutedEvent(
            ""Value2Changed"",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(FooControl));

        public event RoutedEventHandler ↓Value1Changed
        {
            add { this.AddHandler(ValueChangedEvent, value); }
            remove { this.RemoveHandler(Value2ChangedEvent, value); }
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Add uses: 'ValueChangedEvent', remove uses: 'Value2ChangedEvent'."), testCode);
        }

        [Test]
        public static void EventManagerRegisterRoutedEvent()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the Value1Changed event</summary>
        public static readonly RoutedEvent Value1ChangedEvent = EventManager.RegisterRoutedEvent(
            ""Value1Changed"",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(FooControl));

        /// <summary>Identifies the Value2Changed event</summary>
        public static readonly RoutedEvent Value2ChangedEvent = EventManager.RegisterRoutedEvent(
            ""Value2Changed"",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(FooControl));

        public event RoutedEventHandler ↓WrongName
        {
            add { this.AddHandler(Value1ChangedEvent, value); }
            remove { this.RemoveHandler(Value2ChangedEvent, value); }
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [Test]
        public static void EventManagerRegisterRoutedEventExpressionBodies()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the Value1Changed event</summary>
        public static readonly RoutedEvent Value1ChangedEvent = EventManager.RegisterRoutedEvent(
            ""Value1Changed"",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(FooControl));

        /// <summary>Identifies the Value2Changed event</summary>
        public static readonly RoutedEvent Value2ChangedEvent = EventManager.RegisterRoutedEvent(
            ""Value2Changed"",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(FooControl));

        public event RoutedEventHandler Value1Changed
        {
            add => this.AddHandler(Value1ChangedEvent, value);
            remove => this.RemoveHandler(↓Value2ChangedEvent, value);
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }
    }
}
