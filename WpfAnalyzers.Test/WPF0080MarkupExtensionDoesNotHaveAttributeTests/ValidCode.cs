namespace WpfAnalyzers.Test.WPF0080MarkupExtensionDoesNotHaveAttributeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class ValidCode
    {
        private static readonly DiagnosticAnalyzer Analyzer = new WPF0080MarkupExtensionDoesNotHaveAttribute();

        [Test]
        public void WhenHasAttribute()
        {
            var testCode = @"
namespace RoslynSandbox
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
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenNotOverridingProvideValue()
        {
            var testCode = @"
namespace RoslynSandbox
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
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenAbstract()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Markup;

    public abstract class AbstractFooExtension : MarkupExtension
    {
    }
}";
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenIsGeneric()
        {
            var testCode = @"
namespace RoslynSandbox
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
            RoslynAssert.Valid(Analyzer, testCode);
        }
    }
}
