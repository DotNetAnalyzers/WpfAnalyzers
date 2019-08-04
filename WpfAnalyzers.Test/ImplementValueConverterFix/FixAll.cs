namespace WpfAnalyzers.Test
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using NUnit.Framework;

    public class FixAll
    {
        private static readonly CodeFixProvider Fix = new ImplementValueConverterFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("CS0535");

        [TestCase("FooConverter")]
        [TestCase("BarConverter")]
        public void IValueConverter(string className)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Data;

    public class FooConverter : IValueConverter
    {
    }
}".AssertReplace("FooConverter", className);

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
}".AssertReplace("FooConverter", className);

            RoslynAssert.FixAll(Fix, ExpectedDiagnostic, testCode, after);
        }

        [Test]
        public void FullyQualifiedIValueConverter()
        {
            var before = @"
namespace RoslynSandbox
{
    public class FooConverter : ↓System.Windows.Data.IValueConverter
    {
    }
}";

            var after = @"
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
            RoslynAssert.FixAll(Fix, ExpectedDiagnostic, before, after);
        }

        [TestCase("FooConverter")]
        [TestCase("BarConverter")]
        public void IMultiValueConverter(string className)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Data;

    public class FooConverter : IMultiValueConverter
    {
    }
}".AssertReplace("FooConverter", className);

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
}".AssertReplace("FooConverter", className);

            RoslynAssert.FixAll(Fix, ExpectedDiagnostic, testCode, after);
        }

        [Test]
        public void FullyQualifiedIMultiValueConverter()
        {
            var before = @"
namespace RoslynSandbox
{
    public class FooConverter : ↓System.Windows.Data.IMultiValueConverter
    {
    }
}";

            var after = @"
namespace RoslynSandbox
{
    public class FooConverter : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        object[] System.Windows.Data.IMultiValueConverter.ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotSupportedException($""{nameof(FooConverter)} can only be used in OneWay bindings"");
        }
    }
}";
            RoslynAssert.FixAll(Fix, ExpectedDiagnostic, before, after);
        }
    }
}
