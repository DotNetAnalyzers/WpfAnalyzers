namespace WpfAnalyzers.Test
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public class ConstructorArgumentTests
    {
        [Test]
        public void IsMatchOneCtor()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(string))]
    public class FooExtension : MarkupExtension
    {
        public FooExtension(string text)
        {
            Text = text;
        }

        [ConstructorArgument(""text"")]
        public string Text { get; }

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Text;
        }
    }
}";
            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var attribute = syntaxTree.FindAttribute("ConstructorArgument");
            Assert.AreEqual(true, ConstructorArgument.IsMatch(attribute, out _, out _));
        }

        [Test]
        public void NotMatchOneCtor()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(string))]
    public class FooExtension : MarkupExtension
    {
        public FooExtension(string text)
        {
            Text = text;
        }

        [ConstructorArgument(""WRONG"")]
        public string Text { get; }

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Text;
        }
    }
}";
            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var attribute = syntaxTree.FindAttribute("ConstructorArgument");
            Assert.AreEqual(false, ConstructorArgument.IsMatch(attribute, out _, out var parameterName));
            Assert.AreEqual("text", parameterName);
        }
    }
}
