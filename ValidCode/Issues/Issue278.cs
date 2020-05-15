namespace ValidCode.Issues
{
    using System.Windows;
    using System.Windows.Media;

    public class Issue278 : FrameworkElement
    {
        /// <summary>Identifies the <see cref="Color"/> dependency property.</summary>
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            nameof(Color),
            typeof(Color),
            typeof(Issue278),
            new PropertyMetadata(default(Color)));

        public Color Color
        {
            get => (Color)this.GetValue(ColorProperty);
            set => this.SetValue(ColorProperty, value);
        }

        public void M(Issue278 control, Color? color = null)
        {
            control.SetCurrentValue(ColorProperty, color ?? Colors.HotPink);
        }
    }
}
