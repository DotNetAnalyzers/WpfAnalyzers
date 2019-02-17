namespace WpfAnalyzers.Test.WPF0071ConverterDoesNotHaveAttributeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly ValueConverterAnalyzer Analyzer = new ValueConverterAnalyzer();

        [Test]
        public void WhenHasAttribute()
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenHasFullyQualifiedAttribute()
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IgnorePrivateClass()
        {
            var testCode = @"
namespace RoslynSandbox
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void IgnoreProtectedClass()
        {
            var testCode = @"
namespace RoslynSandbox
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}
