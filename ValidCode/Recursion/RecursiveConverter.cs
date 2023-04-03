// ReSharper disable All
namespace ValidCode.Recursion;

using System;
using System.Globalization;
using System.Windows.Data;

[ValueConversion(typeof(object), typeof(object))]
public sealed class RecursiveConverter : IValueConverter
{
    /// <summary> Gets the default instance </summary>
    public static readonly RecursiveConverter Default = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return this.Convert(value, targetType, parameter, culture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return this.ConvertBack(value, targetType, parameter, culture);
    }
}
