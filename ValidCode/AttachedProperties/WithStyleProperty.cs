// ReSharper disable All
namespace ValidCode.AttachedProperties
{
    using System.Windows;
    using System.Windows.Controls;

    [StyleTypedProperty(Property = "Style", StyleTargetType = typeof(TextBlock))]
    public static class WithStyleProperty
    {
        public static readonly DependencyProperty StyleProperty = DependencyProperty.RegisterAttached(
            "Style",
            typeof(Style),
            typeof(WithStyleProperty),
            new PropertyMetadata(default(Style)));

        /// <summary>Helper for setting <see cref="StyleProperty"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="TextBlock"/> to set <see cref="StyleProperty"/> on.</param>
        /// <param name="value">Style property value.</param>
        public static void SetStyle(TextBlock element, Style value)
        {
            element.SetValue(StyleProperty, value);
        }

        /// <summary>Helper for getting <see cref="StyleProperty"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="TextBlock"/> to read <see cref="StyleProperty"/> from.</param>
        /// <returns>Style property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(TextBlock))]
        public static Style GetStyle(TextBlock element)
        {
            return (Style)element.GetValue(StyleProperty);
        }
    }
}
