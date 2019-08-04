namespace WpfAnalyzers.Test
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using NUnit.Framework;

    public class CodeFix
    {
        private static readonly CodeFixProvider Fix = new ImplementValueConverterFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("CS0535");

        [Test]
        public void IValueConverterConvertBack()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.Windows.Data;

    public class FooConverter : ↓IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.Windows.Data;

    public class FooConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        object IValueConverter.ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotSupportedException($""{nameof(FooConverter)} can only be used in OneWay bindings"");
        }
    }
}";
            RoslynAssert.CodeFix(Fix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public void IMultiValueConverterConvertBack()
        {
            var before = @"
namespace RoslynSandbox
{
    using System.Windows.Data;

    public class FooConverter : ↓IMultiValueConverter
    {
        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}";

            var after = @"
namespace RoslynSandbox
{
    using System.Windows.Data;

    public class FooConverter : IMultiValueConverter
    {
        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        object[] IMultiValueConverter.ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotSupportedException($""{nameof(FooConverter)} can only be used in OneWay bindings"");
        }
    }
}";

            RoslynAssert.CodeFix(Fix, ExpectedDiagnostic, before, after);
        }
    }
}
