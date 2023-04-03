namespace ValidCode.Repro;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public class TypeB : Image
{
    public Geometry? IconGeometry
    {
        get => (Geometry?)GetValue(IconGeometryProperty);
        // WPF0014 SetValue must use registered type System.Windows.Media.Geometry
        set => SetValue(IconGeometryProperty, value);
    }

    /// <summary>Identifies the <see cref="IconGeometry"/> dependency property.</summary>
    public static readonly DependencyProperty IconGeometryProperty =
        // WPF0010 Default value for 'TypeA.IconGeometryProperty' must be of type System.Windows.Media.Geometry
#pragma warning disable WPF0016 // Default value is shared reference type
        TypeA.IconGeometryProperty.AddOwner(typeof(TypeB), new FrameworkPropertyMetadata(new EllipseGeometry(default, 5, 5)));
#pragma warning restore WPF0016 // Default value is shared reference type
}
