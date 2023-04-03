namespace ValidCode.WithEnum;

using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    /// <summary>Identifies the <see cref="FooEnum"/> dependency property.</summary>
    public static readonly DependencyProperty FooEnumProperty = Foo.FooEnumProperty.AddOwner(
        typeof(FooControl),
        new FrameworkPropertyMetadata(
            FooEnum.Bar,
            FrameworkPropertyMetadataOptions.Inherits));

    public FooEnum FooEnum
    {
        get => (FooEnum)this.GetValue(FooEnumProperty);
        set => this.SetValue(FooEnumProperty, value);
    }
}
