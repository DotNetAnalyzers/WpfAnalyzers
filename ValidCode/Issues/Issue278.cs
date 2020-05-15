namespace ValidCode.Issues
{
    using System.Collections.Generic;
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

        /// <summary>Identifies the <see cref="Colors"/> dependency property.</summary>
        public static readonly DependencyProperty ColorsProperty = DependencyProperty.Register(
            nameof(Colors),
            typeof(List<Color>),
            typeof(Issue278),
            new PropertyMetadata(default(List<Color>)));

        public Color Color
        {
            get => (Color)this.GetValue(ColorProperty);
            set => this.SetValue(ColorProperty, value);
        }

        public List<Color> Colors
        {
            get => (List<Color>)this.GetValue(ColorsProperty);
            set => this.SetValue(ColorsProperty, value);
        }

        public void M(Issue278 control, Color? color = null)
        {
            control.SetCurrentValue(ColorProperty, color ?? System.Windows.Media.Colors.HotPink);
            control.SetCurrentValue(ColorsProperty, new List<Color> { System.Windows.Media.Colors.HotPink });
        }
    }
}
