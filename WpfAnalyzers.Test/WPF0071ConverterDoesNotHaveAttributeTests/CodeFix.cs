namespace WpfAnalyzers.Test.WPF0071ConverterDoesNotHaveAttributeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new ValueConverterAnalyzer();
        private static readonly CodeFixProvider Fix = new ValueConversionAttributeFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0071ConverterDoesNotHaveAttribute.Descriptor);

        [Test]
        public void AddAttributeDirectCast()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    public class ↓CountConverter : IValueConverter
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

            var fixedCode = @"
namespace RoslynSandbox
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void AddAttributeAsCast()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    public class ↓CountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var col = value as ICollection;
            if (col != null)
            {
                return col.Count;
            }

            return 0;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
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
            var col = value as ICollection;
            if (col != null)
            {
                return col.Count;
            }

            return 0;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void AddAttributeIsPattern()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    public class ↓CountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection col)
            {
                return col.Count;
            }

            return 0;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
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
            if (value is ICollection col)
            {
                return col.Count;
            }

            return 0;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void AddAttributeSwitchPattern()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    public class ↓CountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case ICollection col:
                    return col.Count;
                default:
                    return 0;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
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
            switch (value)
            {
                case ICollection col:
                    return col.Count;
                default:
                    return 0;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenReturningThisObjectFields()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class ↓EmptyToVisibilityConverter : IValueConverter
    {
        public static readonly EmptyToVisibilityConverter VisibleWhenEmpty = new EmptyToVisibilityConverter(Visibility.Visible, Visibility.Collapsed);
        public static readonly EmptyToVisibilityConverter CollapsedWhenEmpty = new EmptyToVisibilityConverter(Visibility.Collapsed, Visibility.Visible);

        private readonly object whenEmpty;
        private readonly object whenNot;

        public EmptyToVisibilityConverter(Visibility whenEmpty, Visibility whenNot)
        {
            this.whenEmpty = whenEmpty;
            this.whenNot = whenNot;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return this.whenEmpty;
            }

            if (value is ICollection col)
            {
                return col.Count == 0 ? this.whenEmpty : this.whenNot;
            }

            if (value is IEnumerable enumerable)
            {
                return enumerable.GetEnumerator().MoveNext() ? this.whenNot : this.whenEmpty;
            }

            return this.whenEmpty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(Visibility))]
    public class EmptyToVisibilityConverter : IValueConverter
    {
        public static readonly EmptyToVisibilityConverter VisibleWhenEmpty = new EmptyToVisibilityConverter(Visibility.Visible, Visibility.Collapsed);
        public static readonly EmptyToVisibilityConverter CollapsedWhenEmpty = new EmptyToVisibilityConverter(Visibility.Collapsed, Visibility.Visible);

        private readonly object whenEmpty;
        private readonly object whenNot;

        public EmptyToVisibilityConverter(Visibility whenEmpty, Visibility whenNot)
        {
            this.whenEmpty = whenEmpty;
            this.whenNot = whenNot;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return this.whenEmpty;
            }

            if (value is ICollection col)
            {
                return col.Count == 0 ? this.whenEmpty : this.whenNot;
            }

            if (value is IEnumerable enumerable)
            {
                return enumerable.GetEnumerator().MoveNext() ? this.whenNot : this.whenEmpty;
            }

            return this.whenEmpty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void Issue188()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Data;

    internal class DecimalDigitsToStringFormatConverter : IValueConverter
    {
        internal static readonly DecimalDigitsToStringFormatConverter Default = new DecimalDigitsToStringFormatConverter();
        private static readonly Dictionary<int, string> Cache = new Dictionary<int, string>();

        private DecimalDigitsToStringFormatConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var i = value as int?;
            if (i == null)
            {
                return null;
            }

            if (Cache.TryGetValue(i.Value, out string format))
            {
                return format;
            }

            if (i >= 0)
            {
                format = ""F"" + i.Value;
                Cache[i.Value] = format;
            }
            else
            {
                format = ""0."" + new string('#', -i.Value);
                Cache[i.Value] = format;
            }

            return format;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($""{this.GetType().Name} does not support use in bindings with Mode = TwoWay."");
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(int?), typeof(string))]
    internal class DecimalDigitsToStringFormatConverter : IValueConverter
    {
        internal static readonly DecimalDigitsToStringFormatConverter Default = new DecimalDigitsToStringFormatConverter();
        private static readonly Dictionary<int, string> Cache = new Dictionary<int, string>();

        private DecimalDigitsToStringFormatConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var i = value as int?;
            if (i == null)
            {
                return null;
            }

            if (Cache.TryGetValue(i.Value, out string format))
            {
                return format;
            }

            if (i >= 0)
            {
                format = ""F"" + i.Value;
                Cache[i.Value] = format;
            }
            else
            {
                format = ""0."" + new string('#', -i.Value);
                Cache[i.Value] = format;
            }

            return format;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($""{this.GetType().Name} does not support use in bindings with Mode = TwoWay."");
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, allowCompilationErrors: AllowCompilationErrors.Yes);
        }

        [Test]
        public void Issue189()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    internal class FormattedTextBlockMarginConverter : IValueConverter
    {
        internal static readonly FormattedTextBlockMarginConverter Default = new FormattedTextBlockMarginConverter();

        private FormattedTextBlockMarginConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var presenter = (ScrollContentPresenter)parameter;
            var presenterMargin = presenter?.Margin;
            var textMargin = ((FrameworkElement)presenter?.Content)?.Margin;
            if (presenterMargin == null || textMargin == null)
            {
#if DEBUG
                throw new InvalidOperationException(""Failed getting formatted text margin."");
#else
                return new Thickness(2, 0, 2, 0);
#endif
            }

            var result = new Thickness(
                presenterMargin.Value.Left + textMargin.Value.Left,
                presenterMargin.Value.Top + textMargin.Value.Top,
                presenterMargin.Value.Right + textMargin.Value.Right,
                presenterMargin.Value.Bottom + textMargin.Value.Bottom);
            return result;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($""{this.GetType().Name} does not support use in bindings with Mode = TwoWay."");
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    [ValueConversion(typeof(object), typeof(Thickness))]
    internal class FormattedTextBlockMarginConverter : IValueConverter
    {
        internal static readonly FormattedTextBlockMarginConverter Default = new FormattedTextBlockMarginConverter();

        private FormattedTextBlockMarginConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var presenter = (ScrollContentPresenter)parameter;
            var presenterMargin = presenter?.Margin;
            var textMargin = ((FrameworkElement)presenter?.Content)?.Margin;
            if (presenterMargin == null || textMargin == null)
            {
#if DEBUG
                throw new InvalidOperationException(""Failed getting formatted text margin."");
#else
                return new Thickness(2, 0, 2, 0);
#endif
            }

            var result = new Thickness(
                presenterMargin.Value.Left + textMargin.Value.Left,
                presenterMargin.Value.Top + textMargin.Value.Top,
                presenterMargin.Value.Right + textMargin.Value.Right,
                presenterMargin.Value.Bottom + textMargin.Value.Bottom);
            return result;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($""{this.GetType().Name} does not support use in bindings with Mode = TwoWay."");
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, allowCompilationErrors: AllowCompilationErrors.Yes);
        }

        [Test]
        public void NotNullReturnsTrueConverter()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public sealed class NotNullReturnsTrueConverter : IValueConverter
    {
        public static readonly NotNullReturnsTrueConverter Default = new NotNullReturnsTrueConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotSupportedException($""{this.GetType().Name} can only be used in oneway bindings"");
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(object), typeof(bool))]
    public sealed class NotNullReturnsTrueConverter : IValueConverter
    {
        public static readonly NotNullReturnsTrueConverter Default = new NotNullReturnsTrueConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotSupportedException($""{this.GetType().Name} can only be used in oneway bindings"");
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void NullToVisibiltyConverter()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public sealed class NullToVisibiltyConverter : IValueConverter
    {
        public static readonly NullToVisibiltyConverter VisibleWhenNull = new NullToVisibiltyConverter(Visibility.Visible, Visibility.Hidden);
        public static readonly NullToVisibiltyConverter HiddenWhenNull = new NullToVisibiltyConverter(Visibility.Hidden, Visibility.Visible);

        private readonly object whenNull;
        private readonly object whenNot;

        private NullToVisibiltyConverter(Visibility whenNull, Visibility whenNot)
        {
            this.whenNull = whenNull;
            this.whenNot = whenNot;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? whenNull : whenNot;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotSupportedException($""{this.GetType().Name} can only be used in one way bindings"");
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    [ValueConversion(typeof(object), typeof(Visibility))]
    public sealed class NullToVisibiltyConverter : IValueConverter
    {
        public static readonly NullToVisibiltyConverter VisibleWhenNull = new NullToVisibiltyConverter(Visibility.Visible, Visibility.Hidden);
        public static readonly NullToVisibiltyConverter HiddenWhenNull = new NullToVisibiltyConverter(Visibility.Hidden, Visibility.Visible);

        private readonly object whenNull;
        private readonly object whenNot;

        private NullToVisibiltyConverter(Visibility whenNull, Visibility whenNot)
        {
            this.whenNull = whenNull;
            this.whenNot = whenNot;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? whenNull : whenNot;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotSupportedException($""{this.GetType().Name} can only be used in one way bindings"");
        }
    }
}";

            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}
