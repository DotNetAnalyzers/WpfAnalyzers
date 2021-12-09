// ReSharper disable All
namespace ValidCode.Recursion
{
    using System.Windows;
    using System.Windows.Controls;

    public class WithDependencyProperties : Control
    {
#pragma warning disable WPF0023 // The callback is trivial, convert to lambda
        /// <summary>Identifies the <see cref="P1"/> dependency property.</summary>
        public static readonly DependencyProperty P1Property = DependencyProperty.Register(
           nameof(P1),
           typeof(int),
           typeof(WithDependencyProperties),
           new PropertyMetadata(
               CreateDefault(),
               (d, e) => OnP1Changed(e.OldValue, e.NewValue),
               (d, e) => CoerceP1(d, e)),
           ValidateP1);
#pragma warning restore WPF0023 // The callback is trivial, convert to lambda

        /// <summary>Identifies the <see cref="P2"/> dependency property.</summary>
        public static readonly DependencyProperty P2Property = DependencyProperty.Register(
            nameof(P2),
            typeof(int),
            typeof(WithDependencyProperties),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty P3Property = DependencyProperty.RegisterAttached(
            "P3", typeof(int), typeof(WithDependencyProperties), new PropertyMetadata(default(int)));

        public int P1
        {
            get => (int)this.GetValue(P1Property);
            set => this.SetValue(P1Property, value);
        }

        public int P2
        {
            get { return this.P2; }
#pragma warning disable WPF0041 // Set mutable dependency properties using SetCurrentValue.
            set { this.P2 = value; }
#pragma warning restore WPF0041 // Set mutable dependency properties using SetCurrentValue.
        }

        public static void SetP3(DependencyObject element, int value) => SetP3(element, value);

        public static int GetP3(DependencyObject element) => GetP3(element);

        private static int CreateDefault() => CreateDefault();

        private static void OnP1Changed(object oldValue, object newValue)
        {
            OnP1Changed(oldValue, newValue);
        }

        private static object CoerceP1(DependencyObject d, object? baseValue) => CoerceP1(d, baseValue);

        private static bool ValidateP1(object? value) => ValidateP1(value);
    }
}
