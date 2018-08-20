namespace WpfAnalyzers.Test.WPF0070ConverterDoesNotHaveDefaultFieldTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class HappyPath
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ValueConverterAnalyzer();

        [Test]
        public void WhenDefaultField()
        {
            var testCode = @"
namespace RoslynSandbox
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
            if (value is ICollection c)
            {
                return c.Count;
            }

            if (value is IEnumerable e)
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenTwoFields()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    [ValueConversion(typeof(bool?), typeof(Visibility))]
    public sealed class BooleanToVisibilityWithNullConverter : IValueConverter
    {
        public static readonly BooleanToVisibilityWithNullConverter CollapsedWhenFalseOrNull = new BooleanToVisibilityWithNullConverter(true);
        public static readonly BooleanToVisibilityWithNullConverter CollapsedWhenTrueOrNull = new BooleanToVisibilityWithNullConverter(false);

        private readonly bool? _visibleWhen;

        public BooleanToVisibilityWithNullConverter()
            : this(true)
        {
        }

        public BooleanToVisibilityWithNullConverter(bool? visibleWhen)
        {
            _visibleWhen = visibleWhen;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Equals(value, _visibleWhen))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenDefaultProperty()
        {
            var testCode = @"
namespace RoslynSandbox
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
            if (value is ICollection c)
            {
                return c.Count;
            }

            if (value is IEnumerable e)
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenMarkupExtension()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    [ValueConversion(typeof(bool?), typeof(Visibility))]
    [MarkupExtensionReturnType(typeof(IValueConverter))]
    public class BooleanToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public Visibility WhenTrue { get; set; } = Visibility.Visible;

        public Visibility WhenFalse { get; set; } = Visibility.Hidden;

        public Visibility WhenNull { get; set; } = Visibility.Hidden;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b
                    ? WhenTrue
                    : WhenFalse;
            }

            if (value != null)
            {
                return value;
            }

            return WhenNull;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenHasMutableMembersExtension()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    [ValueConversion(typeof(bool?), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public Visibility WhenTrue { get; set; } = Visibility.Visible;

        public Visibility WhenFalse { get; set; } = Visibility.Hidden;

        public Visibility WhenNull { get; set; } = Visibility.Hidden;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b
                    ? WhenTrue
                    : WhenFalse;
            }

            if (value != null)
            {
                return value;
            }

            return WhenNull;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenAbstract()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(int))]
    public abstract class CountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection c)
            {
                return c.Count;
            }

            if (value is IEnumerable e)
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenVirtual()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(int))]
    public class CountConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection c)
            {
                return c.Count;
            }

            if (value is IEnumerable e)
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenConstructorWithParameters()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(object), typeof(object))]
    public class FooConverter : IValueConverter
    {
        private readonly object value;

        public FooConverter(object value)
        {
            this.value = value;
        }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return this.value;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void InternalIMultiValueConverterWithDefaultField()
        {
            var testCode = @"
namespace Gu.Wpf.ToolTips
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    internal class IsAnyTrueConverter : IMultiValueConverter
    {
        internal static readonly IsAnyTrueConverter Default = new IsAnyTrueConverter();

        private IsAnyTrueConverter()
        {
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
            {
                return false;
            }

            var result = values.Any(x => Equals(x, BoolBoxes.True))
                                ? BoolBoxes.True
                                : BoolBoxes.False;
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenConstructorParameter()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public sealed class FooConverter : IMultiValueConverter
    {
        public FooConverter(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            => Value;

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}
