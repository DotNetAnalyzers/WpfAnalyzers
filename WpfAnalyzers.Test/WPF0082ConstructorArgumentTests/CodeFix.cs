namespace WpfAnalyzers.Test.WPF0082ConstructorArgumentTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly AttributeAnalyzer Analyzer = new();
        private static readonly ConstructorArgumentAttributeArgumentFix Fix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0082ConstructorArgument);

        [Test]
        public static void Message()
        {
            var code = @"
namespace N
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

        [ConstructorArgument(↓""meh"")]
        public string Text { get; }

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Text;
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("[ConstructorArgument] must match. Expected: text"), code);
        }

        [Test]
        public static void WhenWrongArgument()
        {
            var before = @"
namespace N
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

        [ConstructorArgument(↓""WRONG"")]
        public string Text { get; }

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Text;
        }
    }
}";

            var after = @"
namespace N
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
        }
    }
}
