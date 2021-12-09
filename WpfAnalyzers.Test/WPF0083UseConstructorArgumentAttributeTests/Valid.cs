namespace WpfAnalyzers.Test.WPF0083UseConstructorArgumentAttributeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly WPF0083UseConstructorArgumentAttribute Analyzer = new();

        [Test]
        public static void WhenHasAttribute()
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

        [ConstructorArgument(""text"")]
        public string Text { get; }

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Text;
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void WhenNoAttribute()
        {
            var code = @"
namespace N
{
    using System;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(string))]
    public class FooExtension : MarkupExtension
    {
        public string? Text { get; set; }

        /// <inheritdoc />
        public override object? ProvideValue(IServiceProvider serviceProvider)
        {
            return Text;
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
