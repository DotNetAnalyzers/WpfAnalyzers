namespace WpfAnalyzers.Test.WPF0074DefaultMemberOfWrongTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ValueConverterAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0074DefaultMemberOfWrongType.Descriptor);

        [Test]
        public static void DefaultField()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(int))]
    public class CountConverter : IValueConverter
    {
        public static readonly IValueConverter Default = ↓new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = value as ICollection;
            if (collection != null)
            {
                return collection.Count;
            }

            var e = value as IEnumerable;
            if (e != null)
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

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [Test]
        public static void DefaultProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(int))]
    public class CountConverter : IValueConverter
    {
        public static IValueConverter Default { get; } = ↓new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = value as ICollection;
            if (collection != null)
            {
                return collection.Count;
            }

            var e = value as IEnumerable;
            if (e != null)
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

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }
    }
}
