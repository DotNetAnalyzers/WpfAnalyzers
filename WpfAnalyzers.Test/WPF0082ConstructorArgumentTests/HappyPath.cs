namespace WpfAnalyzers.Test.WPF0082ConstructorArgumentTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class HappyPath
    {
        private static readonly WPF0082ConstructorArgument Analyzer = new WPF0082ConstructorArgument();

        [Test]
        public void WhenPropertyHasAttribute()
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenPropertyWithBackingFieldHasAttribute()
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
            this.Text = text;
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenPropertyWithBackingFieldAssignedBackingFieldHasAttribute()
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
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void WhenNoAttribute()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(string))]
    public class FooExtension : MarkupExtension
    {
        public string Text { get; set; }

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Text;
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public void Issue185()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Windows.Markup;

    /// <summary>
    /// Markup extension for getting Enum.GetValues(this.Type)
    /// </summary>
    [MarkupExtensionReturnType(typeof(Array))]
    public class EnumValuesForExtension : MarkupExtension
    {
        private Type type;

        /// <summary>
        /// Initializes a new instance of the <see cref=""EnumValuesForExtension""/> class.
        /// </summary>
        /// <param name=""type"">The enum type.</param>
        public EnumValuesForExtension(Type type)
        {
            this.type = type;
        }

        /// <summary>
        /// The enum type.
        /// </summary>
        [ConstructorArgument(""type"")]
        public Type Type
        {
            get => this.type;

            set
            {
                this.type = value;
            }
        }

        /// <inheritdoc/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(this.Type);
        }
    }
}";
            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}
