namespace ValidCode.WithEnum
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty FooEnumProperty = DependencyProperty.RegisterAttached(
            "FooEnum",
            typeof(FooEnum),
            typeof(Foo),
            new FrameworkPropertyMetadata(FooEnum.Baz, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>Helper for setting <see cref="FooEnumProperty"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to set <see cref="FooEnumProperty"/> on.</param>
        /// <param name="value">FooEnum property value.</param>
        public static void SetFooEnum(DependencyObject element, FooEnum value)
        {
            element.SetValue(FooEnumProperty, value);
        }

        /// <summary>Helper for getting <see cref="FooEnumProperty"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to read <see cref="FooEnumProperty"/> from.</param>
        /// <returns>FooEnum property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static FooEnum GetFooEnum(DependencyObject element)
        {
            return (FooEnum)element.GetValue(FooEnumProperty);
        }
    }
}
