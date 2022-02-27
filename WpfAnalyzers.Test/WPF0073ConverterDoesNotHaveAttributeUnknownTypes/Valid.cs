namespace WpfAnalyzers.Test.WPF0073ConverterDoesNotHaveAttributeUnknownTypes;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly ValueConverterAnalyzer Analyzer = new();

    [Test]
    public static void WhenHasAttribute()
    {
        var code = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(int))]
    public sealed class CountConverter : IValueConverter
    {
        public static readonly CountConverter Default = new CountConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection c)
            {
                return c.Count;
            }

            if (value is IEnumerable e)
            {
                var num = 0;
                var enumerator = e.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    checked
                    {
                        ++num;
                    }
                }

                (enumerator as IDisposable)?.Dispose();
                return num;
            }

            return -1;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void WhenHasFullyQualifiedAttribute()
    {
        var code = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;

    [System.Windows.Data.ValueConversion(typeof(IEnumerable), typeof(int))]
    public sealed class CountConverter : System.Windows.Data.IValueConverter
    {
        public static readonly CountConverter Default = new CountConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection c)
            {
                return c.Count;
            }

            if (value is IEnumerable e)
            {
                var num = 0;
                var enumerator = e.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    checked
                    {
                        ++num;
                    }
                }

                (enumerator as IDisposable)?.Dispose();
                return num;
            }

            return -1;
        }

        object System.Windows.Data.IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void IgnorePrivateClass()
    {
        var code = @"
namespace N
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class Foo
    {
        private class FooConverter : IValueConverter
        {
            internal static readonly FooConverter Default = new FooConverter();

            private FooConverter()
            {
            }

            public object Convert(object value, Type _, object __, CultureInfo ___)
            {
                return ((int)value).ToString();
            }

            public object ConvertBack(object _, Type __, object ___, CultureInfo ____)
            {
                throw new NotSupportedException();
            }
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void IgnoreProtectedClass()
    {
        var code = @"
namespace N
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class Foo
    {
        protected class FooConverter : IValueConverter
        {
            internal static readonly FooConverter Default = new FooConverter();

            private FooConverter()
            {
            }

            public object Convert(object value, Type _, object __, CultureInfo ___)
            {
                return ((int)value).ToString();
            }

            public object ConvertBack(object _, Type __, object ___, CultureInfo ____)
            {
                throw new NotSupportedException();
            }
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void WhenHasAttributePartial()
    {
        var code = @"
namespace N
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(object), typeof(object))]
    partial class C : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    partial class C
    {
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void WhenHasAttributePartialNullable()
    {
        var code = @"
namespace N
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(object), typeof(object))]
    partial class C : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }
    }

    partial class C
    {
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void Issue249()
    {
        var code = @"
namespace WpfCopyDeploy
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

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void DonNotWarnWhenGeneric()
    {
        var code = @"
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
            _ => throw new ArgumentException($""Expected value of type {typeof(TSource).Name}""),
        };
    }

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException($""{nameof(RelayConverter<TSource, TResult>)} can only be used in OneWay bindings"");
    }
}
";
        RoslynAssert.Valid(Analyzer, code);
    }
}
