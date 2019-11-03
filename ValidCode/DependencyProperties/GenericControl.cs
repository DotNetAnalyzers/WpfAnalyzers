// ReSharper disable All
namespace ValidCode.DependencyProperties
{
    using System.Windows;
    using System.Windows.Controls;

    public class GenericControl<T> : Control
    {
        /// <summary>Identifies the <see cref="Generic"/> dependency property.</summary>
        public static readonly DependencyProperty GenericProperty = DependencyProperty.Register(
            nameof(Generic),
            typeof(T),
            typeof(GenericControl<T>),
            new PropertyMetadata(
                default(T),
                OnGenericChanged));

        public static readonly DependencyProperty GenericAttachedProperty = DependencyProperty.RegisterAttached(
            "GenericAttached", 
            typeof(T), 
            typeof(GenericControl<T>),
            new PropertyMetadata(default(T)));

        public T Generic
        {
            get => (T)this.GetValue(GenericProperty);
            set => this.SetValue(GenericProperty, value);
        }

        /// <summary>Helper for setting <see cref="GenericAttachedProperty"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to set <see cref="GenericAttachedProperty"/> on.</param>
        /// <param name="value">GenericAttached property value.</param>
        public static void SetGenericAttached(DependencyObject element, T value)
        {
            element.SetValue(GenericAttachedProperty, value);
        }

        /// <summary>Helper for getting <see cref="GenericAttachedProperty"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to read <see cref="GenericAttachedProperty"/> from.</param>
        /// <returns>GenericAttached property value.</returns>
        public static T GetGenericAttached(DependencyObject element)
        {
            return (T)element.GetValue(GenericAttachedProperty);
        }

        private static void OnGenericChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (T)e.NewValue;
        }
    }
}
