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
            AnalyzerAssert.Valid(Analyzer, testCode);
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
            AnalyzerAssert.Valid(Analyzer, testCode);
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}
