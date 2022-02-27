namespace WpfAnalyzers.Test.WPF0083UseConstructorArgumentAttributeTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class CodeFix
{
    private static readonly WPF0083UseConstructorArgumentAttribute Analyzer = new();
    private static readonly ConstructorArgumentAttributeFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0083UseConstructorArgumentAttribute);

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

        public string ↓Text { get; }

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Text;
        }
    }
}";

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Add [ConstructorArgument(\"text\"]"), code);
    }

    [Test]
    public static void WhenMissing()
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

        public string ↓Text { get; }

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

    [Test]
    public static void WhenMissingAssigningBackingField()
    {
        var before = @"
namespace N
{
    using System;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(string))]
    public class FooExtension : MarkupExtension
    {
        private string text;

        public FooExtension(string text)
        {
            this.text = text;
        }

        public string ↓Text
        {
            get { return this.text; }
            set { this.text = value; }
        }

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this.Text;
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
        private string text;

        public FooExtension(string text)
        {
            this.text = text;
        }

        [ConstructorArgument(""text"")]
        public string Text
        {
            get { return this.text; }
            set { this.text = value; }
        }

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this.Text;
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }
}
