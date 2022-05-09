namespace ValidCode.DependencyProperties;

using System;
using System.Windows;

public class Chart : FrameworkElement
{
    /// <summary>Identifies the <see cref="Time"/> dependency property.</summary>
    public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
        nameof(Time),
        typeof(DateTimeOffset),
        typeof(Chart),
        new FrameworkPropertyMetadata(
            default(DateTimeOffset),
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            propertyChangedCallback: null,
            coerceValueCallback: (_, o) => Date.Min(DateTimeOffset.Now, (DateTimeOffset)o)));

    public DateTimeOffset Time
    {
        get => (DateTimeOffset)this.GetValue(TimeProperty);
        set => this.SetValue(TimeProperty, value);
    }
}
