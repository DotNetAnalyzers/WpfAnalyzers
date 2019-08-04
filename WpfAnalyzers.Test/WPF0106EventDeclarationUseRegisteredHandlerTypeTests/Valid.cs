namespace WpfAnalyzers.Test.WPF0106EventDeclarationUseRegisteredHandlerTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly RoutedEventEventDeclarationAnalyzer Analyzer = new RoutedEventEventDeclarationAnalyzer();

        [Test]
        public static void EventManagerRegisterRoutedEvent()
        {
            var testCode = @"
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
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void EventManagerRegisterRoutedEventCustomHandler()
        {
            var eventArgsCode = @"namespace N
{
    using System.Windows;

    public class ValueChangedEventArgs<T> : RoutedEventArgs
    {
        public ValueChangedEventArgs(T oldValue, T newValue)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        public ValueChangedEventArgs(T oldValue, T newValue, RoutedEvent routedEvent)
            : base(routedEvent)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        public ValueChangedEventArgs(T oldValue, T newValue, RoutedEvent routedEvent, object source)
            : base(routedEvent, source)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        public T OldValue { get; }

        public T NewValue { get; }
    }
}";

            var delegateCode = @"namespace N
{
    public delegate void ValueChangedEventHandler<T>(object sender, ValueChangedEventArgs<T> e);
}";
            var testCode = @"
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
            RoslynAssert.Valid(Analyzer, eventArgsCode, delegateCode, testCode);
        }
    }
}
