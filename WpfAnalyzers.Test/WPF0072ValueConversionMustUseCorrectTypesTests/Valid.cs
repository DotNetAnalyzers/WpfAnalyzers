namespace WpfAnalyzers.Test.WPF0072ValueConversionMustUseCorrectTypesTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly ValueConverterAnalyzer Analyzer = new ValueConverterAnalyzer();

        [Test]
        public static void WhenHasAttribute()
        {
            var testCode = @"
namespace RoslynSandbox
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
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void WhenHasFullyQualifiedAttribute()
        {
            var testCode = @"
namespace RoslynSandbox
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
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void WhenHasAttributeNegatedNullableBoolean()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(bool?), typeof(bool?))]
    public sealed class InvertBooleanConverter : IValueConverter
    {
        public static readonly InvertBooleanConverter Default = new InvertBooleanConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool?)value;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void WhenReturningUnderscoreObjectFields()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(Visibility))]
    public class EmptyToVisibilityConverter : IValueConverter
    {
        public static readonly EmptyToVisibilityConverter VisibleWhenEmpty = new EmptyToVisibilityConverter(Visibility.Visible, Visibility.Collapsed);
        public static readonly EmptyToVisibilityConverter CollapsedWhenEmpty = new EmptyToVisibilityConverter(Visibility.Collapsed, Visibility.Visible);

        private readonly object _whenEmpty;
        private readonly object _whenNot;

        public EmptyToVisibilityConverter(Visibility whenEmpty, Visibility whenNot)
        {
            _whenEmpty = whenEmpty;
            _whenNot = whenNot;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return _whenEmpty;
            }

            if (value is ICollection col)
            {
                return col.Count == 0 ? _whenEmpty : _whenNot;
            }

            if (value is IEnumerable enumerable)
            {
                return enumerable.GetEnumerator().MoveNext() ? _whenNot : _whenEmpty;
            }

            return _whenEmpty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void WhenReturningThisObjectFields()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(Visibility))]
    public class EmptyToVisibilityConverter : IValueConverter
    {
        public static readonly EmptyToVisibilityConverter VisibleWhenEmpty = new EmptyToVisibilityConverter(Visibility.Visible, Visibility.Collapsed);
        public static readonly EmptyToVisibilityConverter CollapsedWhenEmpty = new EmptyToVisibilityConverter(Visibility.Collapsed, Visibility.Visible);

        private readonly object whenEmpty;
        private readonly object whenNot;

        public EmptyToVisibilityConverter(Visibility whenEmpty, Visibility whenNot)
        {
            this.whenEmpty = whenEmpty;
            this.whenNot = whenNot;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return this.whenEmpty;
            }

            if (value is ICollection col)
            {
                return col.Count == 0 ? this.whenEmpty : this.whenNot;
            }

            if (value is IEnumerable enumerable)
            {
                return enumerable.GetEnumerator().MoveNext() ? this.whenNot : this.whenEmpty;
            }

            return this.whenEmpty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }
    }
}
