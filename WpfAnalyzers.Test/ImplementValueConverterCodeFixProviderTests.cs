// ReSharper disable InconsistentNaming
namespace WpfAnalyzers.Test
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public class ImplementValueConverterCodeFixProviderTests
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ExpectedDiagnostic CS0535 = ExpectedDiagnostic.Create(nameof(CS0535));

        [Test]
        public void IValueConverter()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Data;

    public class FooConverter : IValueConverter
    {
    }
}";

            var fixedCode = @"
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
            AnalyzerAssert.FixAll<ImplementValueConverterCodeFixProvider>(CS0535, testCode, fixedCode, "Implement IValueConverter for one way bindings.");
        }

        [Test]
        public void FullyQualifiedIValueConverter()
        {
            var testCode = @"
namespace RoslynSandbox
{
    public class FooConverter : System.Windows.Data.IValueConverter
    {
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    public class FooConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        object System.Windows.Data.IValueConverter.ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotSupportedException($""{nameof(FooConverter)} can only be used in OneWay bindings"");
        }
    }
}";
            AnalyzerAssert.FixAll<ImplementValueConverterCodeFixProvider>(CS0535, testCode, fixedCode, "Implement IValueConverter for one way bindings.");
        }
    }
}
