namespace WpfAnalyzers.Test.WPF0081MarkupExtensionReturnTypeMustUseCorrectTypeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();

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
        public static void WhenReturnTypeIsObjectAndAttributeIsMoreSpecific()
        {
            var code = @"
namespace N
{
    using System;
    using System.Windows.Data;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(BindingExpression))]
    public class FooExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new Binding();
            return binding.ProvideValue(serviceProvider);
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("[MarkupExtensionReturnType(typeof(IEnumerable))]")]
        [TestCase("[MarkupExtensionReturnType(typeof(IEnumerable<int>))]")]
        public static void EnumerableExtension(string attribute)
        {
            var code = @"
#pragma warning disable CS8019
namespace N
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(IEnumerable))]
    public class EnumerableExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enumerable.Empty<int>();
        }
    }
}".AssertReplace("[MarkupExtensionReturnType(typeof(IEnumerable))]", attribute);
            RoslynAssert.Valid(Analyzer, code);
        }

        [TestCase("[MarkupExtensionReturnType(typeof(IdExtension))]")]
        [TestCase("[MarkupExtensionReturnType(typeof(MarkupExtension))]")]
        [TestCase("[MarkupExtensionReturnType(typeof(IDisposable))]")]
        public static void IdExtension(string attribute)
        {
            var code = @"
namespace N
{
    using System;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(IdExtension))]
    public class IdExtension : MarkupExtension, IDisposable
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public void Dispose() { }
    }
}".AssertReplace("[MarkupExtensionReturnType(typeof(IdExtension))]", attribute);
            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
