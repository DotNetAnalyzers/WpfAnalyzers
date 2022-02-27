// ReSharper disable InconsistentNaming
namespace WpfAnalyzers.Test.ImplementValueConverterFixTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class CodeFix
{
    private static readonly ImplementValueConverterFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("CS0535");

    [Test]
    public static void IValueConverterConvertBack()
    {
        var before = @"
namespace N
{
    using System.Windows.Data;

    public class C : ↓IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}";

        var after = @"
namespace N
{
    using System.Windows.Data;

    public class C : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        object IValueConverter.ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotSupportedException($""{nameof(C)} can only be used in OneWay bindings"");
        }
    }
}";
        RoslynAssert.CodeFix(Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void IMultiValueConverterConvertBack()
    {
        var before = @"
namespace N
{
    using System.Windows.Data;

    public class C : ↓IMultiValueConverter
    {
        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}";

        var after = @"
namespace N
{
    using System.Windows.Data;

    public class C : IMultiValueConverter
    {
        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        object[] IMultiValueConverter.ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotSupportedException($""{nameof(C)} can only be used in OneWay bindings"");
        }
    }
}";

        RoslynAssert.CodeFix(Fix, ExpectedDiagnostic, before, after);
    }
}