namespace ValidCode.DependencyProperties
{
    using System.Windows;
    using System.Windows.Controls;

    [StyleTypedProperty(Property = nameof(BarStyle), StyleTargetType = typeof(Control))]
    public class WithStyleTypedProperty : Control
    {
        /// <summary>Identifies the <see cref="BarStyle"/> dependency property.</summary>
        public static readonly DependencyProperty BarStyleProperty = DependencyProperty.Register(
            nameof(BarStyle),
            typeof(Style),
            typeof(WithStyleTypedProperty),
            new PropertyMetadata(default(Style)));

        public Style? BarStyle
        {
            get => (Style?)this.GetValue(BarStyleProperty);
            set => this.SetValue(BarStyleProperty, value);
        }
    }
}
