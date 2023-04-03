// ReSharper disable All
namespace ValidCode.Converters;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

[ContentProperty(nameof(Converters))]
[ValueConversion(typeof(object), typeof(object))]
public class ValueConverterGroup : IValueConverter
{
    public List<IValueConverter> Converters { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => this.Converters
               .Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
