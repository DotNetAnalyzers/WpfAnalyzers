// ReSharper disable All
namespace ValidCode.AttachedProperties
{
    using System;
    using System.Windows;

    public static class WithNullChecks
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(WithNullChecks),
            new PropertyMetadata(default(string)));

        /// <summary>Helper for setting <see cref="TextProperty"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to set <see cref="TextProperty"/> on.</param>
        /// <param name="value">Text property value.</param>
        public static void SetText(DependencyObject element, string value)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            element.SetValue(TextProperty, value);
        }

        /// <summary>Helper for getting <see cref="TextProperty"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to read <see cref="TextProperty"/> from.</param>
        /// <returns>Text property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static string GetText(DependencyObject element)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return (string)element.GetValue(TextProperty);
        }
    }
}
