namespace WpfAnalyzers.Test.WPF0133ContentPropertyTargetTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Diagnostics
{
    private static readonly AttributeAnalyzer Analyzer = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0133ContentPropertyTarget);

    [Test]
    public static void WhenWrong()
    {
        var code = @"
namespace N
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Markup;

    [ContentProperty(↓""WRONG"")]
    [ValueConversion(typeof(object), typeof(object))]
    public class ValueConverterGroup : IValueConverter
    {
        public List<IValueConverter> Converters { get; } = new List<IValueConverter>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => this.Converters
                   .Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
";
        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }
}