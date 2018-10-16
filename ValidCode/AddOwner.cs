namespace ValidCode
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    public class AddOwner : FrameworkElement
    {
        /// <summary>Identifies the <see cref="BorderThickness"/> dependency property.</summary>
        public static readonly DependencyProperty BorderThicknessProperty = Border.BorderThicknessProperty.AddOwner(typeof(AddOwner));

        /// <summary>Identifies the <see cref="FontSize"/> dependency property.</summary>
        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(AddOwner));

        public Thickness BorderThickness
        {
            get => (Thickness)this.GetValue(BorderThicknessProperty);
            set => this.SetValue(BorderThicknessProperty, value);
        }

        public double FontSize
        {
            get => (double)this.GetValue(FontSizeProperty);
            set => this.SetValue(FontSizeProperty, value);
        }
    }
}
