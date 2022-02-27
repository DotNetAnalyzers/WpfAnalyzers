namespace ValidCode.Converters;

using System;
using System.Windows.Data;

public sealed class RelayConverter<TSource, TResult> : IValueConverter
{
    private readonly Func<TSource, TResult> convert;

    public RelayConverter(Func<TSource, TResult> convert)
    {
        this.convert = convert;
    }

    public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return value switch
        {
            TSource source => this.convert(source),
            _ => throw new ArgumentException($"Expected value of type {typeof(TSource).Name}"),
        };
    }

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException($"{nameof(RelayConverter<TSource, TResult>)} can only be used in OneWay bindings");
    }
}
