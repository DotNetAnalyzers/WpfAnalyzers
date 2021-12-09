namespace WpfAnalyzers.Test.WPF0080MarkupExtensionDoesNotHaveAttributeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly WPF0080MarkupExtensionDoesNotHaveAttribute Analyzer = new();

        [Test]
        public static void WhenHasAttribute()
        {
            var code = @"
namespace N
{
    using System;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(FooExtension))]
    public class FooExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void WhenNotOverridingProvideValue()
        {
            var code = @"
namespace N
{
    using System.Reflection;
    using System.Windows;
    using System.Windows.Markup;

    public sealed class Foo : ResourceKey
    {
        public Foo(int id) => ID = id;

        [ConstructorArgument(""id"")]
        public int ID { get; }

        public override Assembly Assembly => typeof(Foo).Assembly;
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void WhenAbstract()
        {
            var code = @"
namespace N
{
    using System.Windows.Markup;

    public abstract class AbstractFooExtension : MarkupExtension
    {
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void WhenIsGeneric()
        {
            var code = @"
#nullable disable
namespace N
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Markup;

    public abstract class ConverterMarkupExtension<T> : MarkupExtension, IValueConverter
        where T : class, new()
    {
        private static T _converter;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter ?? (_converter = new T());
        }

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
