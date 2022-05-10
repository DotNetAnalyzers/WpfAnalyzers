namespace ValidCode.RoutedEvents
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    public class WithCustomEventHandler : Control
    {
        /// <summary>Identifies the <see cref="ValueChanged"/> routed event.</summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
            RoutingStrategy.Direct,
            typeof(ValueChangedEventHandler),
            typeof(WithCustomEventHandler));

        [Browsable(true)]
        public event ValueChangedEventHandler ValueChanged
        {
            add => this.AddHandler(ValueChangedEvent, value);
            remove => this.RemoveHandler(ValueChangedEvent, value);
        }
    }
}

