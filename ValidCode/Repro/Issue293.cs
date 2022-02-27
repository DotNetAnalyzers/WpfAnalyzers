namespace ValidCode.Repro;

using System.Drawing;
using System.Windows;

public class Issue293 : FrameworkElement
{
    /// <summary>Identifies the <see cref="Background"/> dependency property.</summary>
    public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
        nameof(Background),
        typeof(Brush),
        typeof(Issue293),
        new FrameworkPropertyMetadata(
            Brushes.Transparent,
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

    public Brush? Background
    {
        get => (Brush?)this.GetValue(BackgroundProperty);
        set => this.SetValue(BackgroundProperty, value);
    }
}
