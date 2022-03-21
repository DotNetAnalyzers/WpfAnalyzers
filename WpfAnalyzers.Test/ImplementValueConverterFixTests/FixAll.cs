// ReSharper disable InconsistentNaming
namespace WpfAnalyzers.Test.ImplementValueConverterFixTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

[Explicit("Seems like assert hangs")]
public static class FixAll
{
    private static readonly ImplementValueConverterFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("CS0535");

    [Test]
    public static void IValueConverter()
    {
        var before = @"
namespace N
{
    using System.Windows.Data;

    public class C : ↓IValueConverter
    {
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

        RoslynAssert.FixAll(Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void FullyQualifiedIValueConverter()
    {
        var before = @"
namespace N
{
    public class C : ↓System.Windows.Data.IValueConverter
    {
    }
}";

        var after = @"
namespace N
{
    public class C : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        object System.Windows.Data.IValueConverter.ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotSupportedException($""{nameof(C)} can only be used in OneWay bindings"");
        }
    }
}";
        RoslynAssert.FixAll(Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void IMultiValueConverter()
    {
        var before = @"
namespace N
{
    using System.Windows.Data;

    public class C : ↓IMultiValueConverter
    {
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

        RoslynAssert.FixAll(Fix, ExpectedDiagnostic, before, after);
    }
    
    [Test]
    public static void FullyQualifiedIMultiValueConverter()
    {
        var before = @"
namespace N
{
    public class C : ↓System.Windows.Data.IMultiValueConverter
    {
    }
}";

        var after = @"
namespace N
{
    public class C : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        object[] System.Windows.Data.IMultiValueConverter.ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotSupportedException($""{nameof(C)} can only be used in OneWay bindings"");
        }
    }
}";
        RoslynAssert.FixAll(Fix, ExpectedDiagnostic, before, after);
    }
}
