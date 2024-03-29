﻿namespace WpfAnalyzers.Test.WPF0073ConverterDoesNotHaveAttributeUnknownTypes;

using Gu.Roslyn.Asserts;

using NUnit.Framework;

public static class CodeFix
{
    private static readonly ValueConverterAnalyzer Analyzer = new();
    private static readonly ValueConversionAttributeFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0073ConverterDoesNotHaveAttributeUnknownTypes);

    [Test]
    public static void AddAttributeDirectCast()
    {
        var before = @"
#nullable disable
namespace Gu.Wpf.PropertyGrid
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(MultiplyConverter))]
    public class ↓MultiplyConverter : MarkupExtension, IValueConverter
    {
        public MultiplyConverter(double factor)
        {
            this.Factor = factor;
        }

        [ConstructorArgument(""factor"")]
        public double Factor { get; set; }

        /// <inheritdoc/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value is TimeSpan)
            {
                return TimeSpan.FromTicks((long)(this.Factor * ((TimeSpan)value).Ticks));
            }

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Byte:
                    return this.Factor * (byte)value;
                case TypeCode.Int16:
                    return this.Factor * (short)value;
                case TypeCode.UInt16:
                    return this.Factor * (ushort)value;
                case TypeCode.Int32:
                    return this.Factor * (int)value;
                case TypeCode.UInt32:
                    return this.Factor * (uint)value;
                case TypeCode.Int64:
                    return this.Factor * (long)value;
                case TypeCode.UInt64:
                    return this.Factor * (ulong)value;
                case TypeCode.Single:
                    return this.Factor * (float)value;
                case TypeCode.Double:
                    return this.Factor * (double)value;
                case TypeCode.Decimal:
                    return (decimal)(this.Factor * (double)value);
                default:
                    throw new ArgumentOutOfRangeException($""Could not multiply value of type: {value.GetType()}"");
            }
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value is TimeSpan)
            {
                return TimeSpan.FromTicks((long)(((TimeSpan)value).Ticks / this.Factor));
            }

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Byte:
                    return (byte)value / this.Factor;
                case TypeCode.Int16:
                    return (short)value / this.Factor;
                case TypeCode.UInt16:
                    return (ushort)value / this.Factor;
                case TypeCode.Int32:
                    return (int)value / this.Factor;
                case TypeCode.UInt32:
                    return (uint)value / this.Factor;
                case TypeCode.Int64:
                    return (long)value / this.Factor;
                case TypeCode.UInt64:
                    return (ulong)value / this.Factor;
                case TypeCode.Single:
                    return (float)value / this.Factor;
                case TypeCode.Double:
                    return (double)value / this.Factor;
                case TypeCode.Decimal:
                    return (decimal)((double)value / this.Factor);
                default:
                    throw new ArgumentOutOfRangeException($""Could not multiply value of type: {value.GetType()}"");
            }
        }
    }
}";

        var after = @"
#nullable disable
namespace Gu.Wpf.PropertyGrid
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(MultiplyConverter))]
    [ValueConversion(typeof(TYPE), typeof(TYPE))]
    public class MultiplyConverter : MarkupExtension, IValueConverter
    {
        public MultiplyConverter(double factor)
        {
            this.Factor = factor;
        }

        [ConstructorArgument(""factor"")]
        public double Factor { get; set; }

        /// <inheritdoc/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value is TimeSpan)
            {
                return TimeSpan.FromTicks((long)(this.Factor * ((TimeSpan)value).Ticks));
            }

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Byte:
                    return this.Factor * (byte)value;
                case TypeCode.Int16:
                    return this.Factor * (short)value;
                case TypeCode.UInt16:
                    return this.Factor * (ushort)value;
                case TypeCode.Int32:
                    return this.Factor * (int)value;
                case TypeCode.UInt32:
                    return this.Factor * (uint)value;
                case TypeCode.Int64:
                    return this.Factor * (long)value;
                case TypeCode.UInt64:
                    return this.Factor * (ulong)value;
                case TypeCode.Single:
                    return this.Factor * (float)value;
                case TypeCode.Double:
                    return this.Factor * (double)value;
                case TypeCode.Decimal:
                    return (decimal)(this.Factor * (double)value);
                default:
                    throw new ArgumentOutOfRangeException($""Could not multiply value of type: {value.GetType()}"");
            }
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value is TimeSpan)
            {
                return TimeSpan.FromTicks((long)(((TimeSpan)value).Ticks / this.Factor));
            }

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Byte:
                    return (byte)value / this.Factor;
                case TypeCode.Int16:
                    return (short)value / this.Factor;
                case TypeCode.UInt16:
                    return (ushort)value / this.Factor;
                case TypeCode.Int32:
                    return (int)value / this.Factor;
                case TypeCode.UInt32:
                    return (uint)value / this.Factor;
                case TypeCode.Int64:
                    return (long)value / this.Factor;
                case TypeCode.UInt64:
                    return (ulong)value / this.Factor;
                case TypeCode.Single:
                    return (float)value / this.Factor;
                case TypeCode.Double:
                    return (double)value / this.Factor;
                case TypeCode.Decimal:
                    return (decimal)((double)value / this.Factor);
                default:
                    throw new ArgumentOutOfRangeException($""Could not multiply value of type: {value.GetType()}"");
            }
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after, settings: Settings.Default.WithAllowedCompilerDiagnostics(AllowedCompilerDiagnostics.WarningsAndErrors));
    }
}
