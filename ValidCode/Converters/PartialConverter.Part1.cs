namespace ValidCode.Converters;

using System;
using System.Globalization;
using System.Windows.Data;

[ValueConversion(typeof(object), typeof(object))]
sealed partial class PartialConverter : IValueConverter
{
    /// <summary> Gets the default instance </summary>
    static readonly PartialConverter Default = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}
