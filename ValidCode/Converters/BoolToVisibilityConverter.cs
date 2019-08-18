namespace ValidCode.Converters
{
    using System;
    using System.Windows;
    using System.Windows.Data;

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public static readonly BoolToVisibilityConverter VisibleWhenTrue = new BoolToVisibilityConverter(Visibility.Visible, Visibility.Collapsed);
        public static readonly BoolToVisibilityConverter VisibleWhenFalse = new BoolToVisibilityConverter(Visibility.Collapsed, Visibility.Visible);

        private readonly object whenTrue;
        private readonly object whenFalse;

        public BoolToVisibilityConverter(Visibility whenTrue, Visibility whenFalse)
        {
            this.whenTrue = whenTrue;
            this.whenFalse = whenFalse;
        }

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? this.whenTrue : this.whenFalse;
            }

            throw new ArgumentException("Expected a bool", nameof(value));
        }

        object IValueConverter.ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotSupportedException($"{nameof(BoolToVisibilityConverter)} can only be used in OneWay bindings");
        }
    }
}
