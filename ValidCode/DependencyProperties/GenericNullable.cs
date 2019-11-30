// ReSharper disable All
namespace ValidCode.DependencyProperties
{
    using System.Windows;

    public class GenericNullable<T> : FrameworkElement
        where T : struct
    {
        /// <summary>Identifies the <see cref="Value"/> dependency property.</summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(T?),
            typeof(GenericNullable<T>),
            new PropertyMetadata(
                default(T?),
                OnValueChanged));

        public T? Value
        {
            get => (T?)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        public void Update(T value)
        {
            this.SetCurrentValue(ValueProperty, value);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is T oldValue)
            {
            }

            if (e.NewValue is T newValue)
            {
            }
        }
    }
}
