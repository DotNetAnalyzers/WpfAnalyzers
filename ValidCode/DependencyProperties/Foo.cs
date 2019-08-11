// ReSharper disable All
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace ValidCode.DependencyProperties
{
    using System.Windows;

    public static class Foo
    {
        public static readonly DependencyProperty Bar1Property = DependencyProperty.RegisterAttached(
            "Bar1",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        private static readonly DependencyPropertyKey Bar2PropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "Bar2",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty Bar2Property = Bar2PropertyKey.DependencyProperty;

        /// <summary>Helper for setting <see cref="Bar1Property"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="FrameworkElement"/> to set <see cref="Bar1Property"/> on.</param>
        /// <param name="value">Bar1 property value.</param>
        public static void SetBar1(FrameworkElement element, int value)
        {
            element.SetValue(Bar1Property, value);
        }

        /// <summary>Helper for getting <see cref="Bar1Property"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="FrameworkElement"/> to read <see cref="Bar1Property"/> from.</param>
        /// <returns>Bar1 property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static int GetBar1(FrameworkElement element)
        {
            return (int)element.GetValue(Bar1Property);
        }

        /// <summary>Helper for setting <see cref="Bar2PropertyKey"/> on <paramref name="o"/>.</summary>
        /// <param name="o"><see cref="DependencyObject"/> to set <see cref="Bar2PropertyKey"/> on.</param>
        /// <param name="n">Bar2 property value.</param>
        public static void SetBar2(DependencyObject o, int n)
        {
            o.SetValue(Bar2PropertyKey, n);
        }

        /// <summary>Helper for getting <see cref="Bar2Property"/> from <paramref name="o"/>.</summary>
        /// <param name="o"><see cref="DependencyObject"/> to read <see cref="Bar2Property"/> from.</param>
        /// <returns>Bar2 property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static int GetBar2(DependencyObject o)
        {
            return (int)o.GetValue(Bar2Property);
        }
    }
}
