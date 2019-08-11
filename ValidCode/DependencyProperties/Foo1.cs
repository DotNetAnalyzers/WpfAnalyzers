// ReSharper disable All
namespace ValidCode.DependencyProperties
{
    using System.Windows;
    using System.Windows.Controls;

    public static class Foo1
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            "Bar",
            typeof(bool),
            typeof(Foo1),
            new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty OtherProperty = DependencyProperty.RegisterAttached(
            "Other",
            typeof(string),
            typeof(Foo1),
            new FrameworkPropertyMetadata(
                "abc",
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsArrange,
                OnOtherChanged,
                CoerceOther));

        private static readonly DependencyPropertyKey ReadOnlyPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "ReadOnly",
            typeof(bool),
            typeof(Foo1),
            new PropertyMetadata(default(bool)));

        /// <summary>Identifies the <see cref="ReadOnlyProperty"/> dependency property.</summary>
        public static readonly DependencyProperty ReadOnlyProperty = ReadOnlyPropertyKey.DependencyProperty;

        /// <summary>Helper for setting <see cref="BarProperty"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="FrameworkElement"/> to set <see cref="BarProperty"/> on.</param>
        /// <param name="value">Bar property value.</param>
        public static void SetBar(FrameworkElement element, bool value)
        {
            element.SetValue(BarProperty, BooleanBoxes.Box(value));
        }

        /// <summary>Helper for getting <see cref="BarProperty"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="FrameworkElement"/> to read <see cref="BarProperty"/> from.</param>
        /// <returns>Bar property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static bool GetBar(FrameworkElement element)
        {
            return (bool)element.GetValue(BarProperty);
        }

        /// <summary>Helper for setting <see cref="OtherProperty"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to set <see cref="OtherProperty"/> on.</param>
        /// <param name="value">Other property value.</param>
        public static void SetOther(this DependencyObject element, string value)
        {
            element.SetValue(OtherProperty, value);
        }

        /// <summary>Helper for getting <see cref="OtherProperty"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to read <see cref="OtherProperty"/> from.</param>
        /// <returns>Other property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static string GetOther(this DependencyObject element)
        {
            return (string)element.GetValue(OtherProperty);
        }

        private static void SetReadOnly(this Control element, bool value)
        {
            element.SetValue(ReadOnlyPropertyKey, value);
        }

        /// <summary>Helper for getting <see cref="ReadOnlyProperty"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="Control"/> to read <see cref="ReadOnlyProperty"/> from.</param>
        /// <returns>ReadOnly property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(Control))]
        public static bool GetReadOnly(this Control element)
        {
            return (bool)element.GetValue(ReadOnlyProperty);
        }

        private static object CoerceOther(DependencyObject d, object basevalue)
        {
            // very strange stuff here, tests things.
#pragma warning disable WPF0041
            d.SetValue(OtherProperty, basevalue);
#pragma warning restore WPF0041
            return d.GetValue(BarProperty);
        }

        private static void OnOtherChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetCurrentValue(BarProperty, true);
            d.SetValue(ReadOnlyPropertyKey, true);
        }
    }
}
