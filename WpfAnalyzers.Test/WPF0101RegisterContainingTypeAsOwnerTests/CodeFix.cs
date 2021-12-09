namespace WpfAnalyzers.Test.WPF0101RegisterContainingTypeAsOwnerTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly RoutedEventBackingFieldOrPropertyAnalyzer Analyzer = new();
        private static readonly UseContainingTypeFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0101RegisterContainingTypeAsOwner);

        [Test]
        public static void Message()
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the ValueChanged event</summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(↓Window));

        public event RoutedEventHandler ValueChanged
        {
            add { this.AddHandler(ValueChangedEvent, value); }
            remove { this.RemoveHandler(ValueChangedEvent, value); }
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Register containing type: 'FooControl' as owner."), code);
        }

        [Test]
        public static void EventManagerRegisterRoutedEvent()
        {
            var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the ValueChanged event</summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(↓Window));

        public event RoutedEventHandler ValueChanged
        {
            add { this.AddHandler(ValueChangedEvent, value); }
            remove { this.RemoveHandler(ValueChangedEvent, value); }
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
        /// <summary>Identifies the ValueChanged event</summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(FooControl));

        public event RoutedEventHandler ValueChanged
        {
            add { this.AddHandler(ValueChangedEvent, value); }
            remove { this.RemoveHandler(ValueChangedEvent, value); }
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
