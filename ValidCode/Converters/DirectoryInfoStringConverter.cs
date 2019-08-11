namespace ValidCode.Converters
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows.Data;

    [ValueConversion(typeof(DirectoryInfo), typeof(string))]
    public class DirectoryInfoStringConverter : IValueConverter
    {
        public static readonly DirectoryInfoStringConverter Default = new DirectoryInfoStringConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DirectoryInfo directoryInfo)
            {
                return directoryInfo.FullName;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return null;
                }

                return new DirectoryInfo(text);
            }

            return null;
        }
    }
}
