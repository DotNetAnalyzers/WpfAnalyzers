namespace ValidCode.Repro;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public class TypeA : Image
{
    public Geometry? IconGeometry
    {
        get => (Geometry?)GetValue(IconGeometryProperty);
        set => SetValue(IconGeometryProperty, value);
    }

    /// <summary>Identifies the <see cref="IconGeometry"/> dependency property.</summary>
    public static readonly DependencyProperty IconGeometryProperty =
        DependencyProperty.Register(nameof(IconGeometry), typeof(Geometry), typeof(TypeA), new PropertyMetadata(null));
}
