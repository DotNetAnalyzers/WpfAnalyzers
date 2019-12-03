namespace ValidCode.RoutedEvents
{
    using System.Windows;

    public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs e);

    public class ValueChangedEventArgs : RoutedEventArgs
    {
        public ValueChangedEventArgs(double oldValue, double newValue)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        public double OldValue { get; }

        public double NewValue { get; }
    }
}
