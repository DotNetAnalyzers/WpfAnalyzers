namespace WpfAnalyzers.Test.WPF0072ValueConversionMustUseCorrectTypesTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class CodeFix
{
    private static readonly ValueConverterAnalyzer Analyzer = new();
    private static readonly ValueConversionAttributeArgumentFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0072ValueConversionMustUseCorrectTypes);

    [Test]
    public static void MessageWhenWrongSourceType()
    {
        var code = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(↓typeof(string), typeof(int))]
    public class CountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ICollection)value).Count;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("ValueConversion must use correct types Expected: System.Collections.ICollection"), code);
    }

    [Test]
    public static void MessageWhenWrongTargetType()
    {
        var code = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(ICollection), ↓typeof(string))]
    public class CountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ICollection)value).Count;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("ValueConversion must use correct types Expected: int"), code);
    }

    [Test]
    public static void DirectCastWrongSourceType()
    {
        var before = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(↓typeof(string), typeof(int))]
    public class CountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ICollection)value).Count;
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

    [ValueConversion(typeof(ICollection), typeof(int))]
    public class CountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ICollection)value).Count;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DirectCastWrongSourceTypeFullyQualified()
    {
        var before = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;

    [System.Windows.Data.ValueConversion(↓typeof(string), typeof(int))]
    public class CountConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ICollection)value).Count;
        }

        object System.Windows.Data.IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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

    [System.Windows.Data.ValueConversion(typeof(ICollection), typeof(int))]
    public class CountConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ICollection)value).Count;
        }

        object System.Windows.Data.IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DirectCastWrongSourceTypeFullyQualifiedIncludeAttribute()
    {
        var before = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;

    [System.Windows.Data.ValueConversionAttribute(↓typeof(string), typeof(int))]
    public class CountConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ICollection)value).Count;
        }

        object System.Windows.Data.IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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

    [System.Windows.Data.ValueConversionAttribute(typeof(ICollection), typeof(int))]
    public class CountConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ICollection)value).Count;
        }

        object System.Windows.Data.IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void DirectCastWrongTargetType()
    {
        var before = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(ICollection), ↓typeof(string))]
    public class CountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ICollection)value).Count;
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

    [ValueConversion(typeof(ICollection), typeof(int))]
    public class CountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ICollection)value).Count;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }
}