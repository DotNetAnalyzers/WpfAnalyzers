namespace WpfAnalyzers.Test.WPF0083UseConstructorArgumentAttributeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new WPF0083UseConstructorArgumentAttribute();
        private static readonly CodeFixProvider Fix = new ConstructorArgumentAttributeFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0083UseConstructorArgumentAttribute.DiagnosticId);

        [Test]
        public void Message()
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

        public string ↓Text { get; }

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Text;
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Add [ConstructorArgument(\"text\"]"), testCode);
        }

        [Test]
        public void WhenMissing()
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

        public string ↓Text { get; }

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Text;
        }
    }
}";

            var fixedCode = @"
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public void WhenMissingAssigningBackingField()
        {
            var testCode = @"
namespace RoslynSandbox
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

            var fixedCode = @"
namespace RoslynSandbox
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
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}
