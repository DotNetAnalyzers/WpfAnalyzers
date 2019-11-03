// ReSharper disable All
namespace ValidCode.DependencyProperties
{
    using System.Windows;
    using System.Windows.Controls;

    public class GenericControl<T> : Control
    {
        /// <summary>Identifies the <see cref="Bar"/> dependency property.</summary>
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(T),
            typeof(GenericControl<T>),
            new PropertyMetadata(
                default(T),
                OnBarChanged));

        public T Bar
        {
            get => (T)this.GetValue(BarProperty);
            set => this.SetValue(BarProperty, value);
        }

        private static void OnBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (T)e.NewValue;
        }
    }
}
