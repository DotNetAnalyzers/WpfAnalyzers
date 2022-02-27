// ReSharper disable InconsistentNaming
namespace WpfAnalyzers.Test.WPF0070ConverterDoesNotHaveDefaultFieldTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class CodeFix
{
    private static readonly ValueConverterAnalyzer Analyzer = new();
    private static readonly AddDefaultMemberFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0070ConverterDoesNotHaveDefaultField);

    [Test]
    public static void Message()
    {
        var code = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(int))]
    public class ↓CountConverter : IValueConverter
    {
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

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Add default field to converter"), code);
    }

    [Test]
    public static void IValueConverterAddDefaultFieldPublic()
    {
        var before = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(int))]
    public class ↓CountConverter : IValueConverter
    {
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

        var after = @"
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
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, "Add default field.");
    }

    [Test]
    public static void IValueConverterAddDefaultFieldInternal()
    {
        var before = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(int))]
    internal class ↓CountConverter : IValueConverter
    {
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

        var after = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(int))]
    internal sealed class CountConverter : IValueConverter
    {
        internal static readonly CountConverter Default = new CountConverter();

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
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, "Add default field.");
    }

    [Test]
    public static void IValueConverterAddDefaultFieldWhenSealedIssue225()
    {
        var before = @"
namespace N
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    sealed class ↓Foo : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}";

        var after = @"
namespace N
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    sealed class Foo : IValueConverter
    {
        static readonly Foo Default = new Foo();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, "Add default field.");
    }

    [Test]
    public static void IMultiValueConverterAddDefaultField()
    {
        var before = @"
namespace N
{
    public class ↓FooConverter : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        object[] System.Windows.Data.IMultiValueConverter.ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotSupportedException($""{ nameof(FooConverter) } can only be used in OneWay bindings"");
        }
    }
}";

        var after = @"
namespace N
{
    public sealed class FooConverter : System.Windows.Data.IMultiValueConverter
    {
        public static readonly FooConverter Default = new FooConverter();

        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        object[] System.Windows.Data.IMultiValueConverter.ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotSupportedException($""{ nameof(FooConverter) } can only be used in OneWay bindings"");
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, "Add default field.");
    }

    [Test]
    public static void AddDefaultFieldWithDocs()
    {
        var before = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(int))]
    public class ↓CountConverter : IValueConverter
    {
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

        var after = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(int))]
    public sealed class CountConverter : IValueConverter
    {
        /// <summary> Gets the default instance </summary>
        public static readonly CountConverter Default = new CountConverter();

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
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, "Add default field with docs.");
    }

    [Test]
    public static void AddDefaultProperty()
    {
        var before = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(int))]
    public class ↓CountConverter : IValueConverter
    {
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

        var after = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(int))]
    public sealed class CountConverter : IValueConverter
    {
        public static CountConverter Default { get; } = new CountConverter();

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
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, "Add default property.");
    }

    [Test]
    public static void AddDefaultPropertyWithDocs()
    {
        var before = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(int))]
    public class ↓CountConverter : IValueConverter
    {
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

        var after = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(int))]
    public sealed class CountConverter : IValueConverter
    {
        /// <summary> Gets the default instance </summary>
        public static CountConverter Default { get; } = new CountConverter();

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
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, "Add default property with docs.");
    }
}