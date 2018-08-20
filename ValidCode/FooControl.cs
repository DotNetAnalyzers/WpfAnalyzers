// ReSharper disable All
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace ValidCode
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref="Bar1"/> dependency property.</summary>
        public static readonly DependencyProperty Bar1Property = DependencyProperty.Register(
            nameof(Bar1),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        /// <summary>Identifies the <see cref="Bar2"/> dependency property.</summary>
        public static DependencyProperty Bar2Property { get; } = DependencyProperty.Register(
            "Bar2",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        private static readonly DependencyPropertyKey Bar3PropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Bar3),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        /// <summary>Identifies the <see cref="Bar3"/> dependency property.</summary>
        public static readonly DependencyProperty Bar3Property = Bar3PropertyKey.DependencyProperty;

        public static readonly DependencyProperty Bar4Property = DependencyProperty.RegisterAttached(
            "Bar4",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        private static readonly DependencyPropertyKey Bar5PropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "Bar5",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty Bar5Property = Bar5PropertyKey.DependencyProperty;

        static FooControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FooControl), new FrameworkPropertyMetadata(typeof(FooControl)));
        }

        public int Bar1
        {
            get => (int)this.GetValue(Bar1Property);
            set => this.SetValue(Bar1Property, value);
        }

        public int Bar2
        {
            get => (int)GetValue(Bar2Property);
            set => SetValue(Bar2Property, value);
        }

        public int Bar3
        {
            get => (int)this.GetValue(Bar3Property);
            set => this.SetValue(Bar3PropertyKey, value);
        }

        /// <summary>Helper for setting <see cref="Bar4Property"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to set <see cref="Bar4Property"/> on.</param>
        /// <param name="value">Bar4 property value.</param>
        public static void SetBar4(DependencyObject element, int value)
        {
            element.SetValue(Bar4Property, value);
        }

        /// <summary>Helper for getting <see cref="Bar4Property"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="FrameworkElement"/> to read <see cref="Bar4Property"/> from.</param>
        /// <returns>Bar4 property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static int GetBar4(FrameworkElement element)
        {
            return (int)element.GetValue(Bar4Property);
        }

        /// <summary>Helper for setting <see cref="Bar5PropertyKey"/> on <paramref name="o"/>.</summary>
        /// <param name="o"><see cref="FrameworkElement"/> to set <see cref="Bar5PropertyKey"/> on.</param>
        /// <param name="i">Bar5 property value.</param>
        public static void SetBar5(FrameworkElement o, int i)
        {
            o.SetValue(Bar5PropertyKey, i);
        }

        /// <summary>Helper for getting <see cref="Bar5Property"/> from <paramref name="o"/>.</summary>
        /// <param name="o"><see cref="DependencyObject"/> to read <see cref="Bar5Property"/> from.</param>
        /// <returns>Bar5 property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static int GetBar5(DependencyObject o)
        {
            return (int)o.GetValue(Bar5Property);
        }
    }
}
