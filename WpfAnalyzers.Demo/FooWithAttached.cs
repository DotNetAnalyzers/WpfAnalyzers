namespace WpfAnalyzers.Demo
{
    using System.Windows;

    public static class FooWithAttached
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.RegisterAttached(
            "Bar",
            typeof(int),
            typeof(FooWithAttached),
            new PropertyMetadata(default(int)));

        private static readonly DependencyPropertyKey BarReadOnlyPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "BarReadOnly",
            typeof(int),
            typeof(FooWithAttached),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarReadOnlyProperty = BarReadOnlyPropertyKey.DependencyProperty;

        public static void SetBar(this FrameworkElement element, int value)
        {
            element.SetValue(BarProperty, value);
        }

        public static int GetBar(this FrameworkElement element)
        {
            return (int)element.GetValue(BarProperty);
        }

        private static void SetBarReadOnly(this FrameworkElement element, int value)
        {
            element.SetValue(BarReadOnlyPropertyKey, value);
        }

        [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static int GetBarReadOnly(this FrameworkElement element)
        {
            return (int)element.GetValue(BarReadOnlyProperty);
        }
    }
}