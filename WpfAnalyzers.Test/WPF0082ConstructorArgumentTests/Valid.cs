namespace WpfAnalyzers.Test.WPF0082ConstructorArgumentTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();

        [Test]
        public static void WhenPropertyHasAttribute()
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
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void WhenPropertyWithBackingFieldHasAttribute()
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
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void WhenPropertyWithBackingFieldAssignedBackingFieldHasAttribute()
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
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void WhenNoAttribute()
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
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void Issue185()
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
            RoslynAssert.Valid(Analyzer, testCode);
        }

        [Test]
        public static void Issue185WithEnsure()
        {
            var ensureCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public static class Ensure
    {
        public static T NotNull<T>(T value, string parameter, [CallerMemberName] string caller = null)
            where T : class
        {
            Debug.Assert(!string.IsNullOrEmpty(parameter), ""parameter cannot be null"");

            if (value == null)
            {
                var message = $""Expected parameter {parameter} in member {caller} to not be null"";
                throw new ArgumentNullException(parameter, message);
            }

            return value;
        }

        internal static void IsTrue(bool condition, string parameterName, string message)
        {
            Debug.Assert(!string.IsNullOrEmpty(parameterName), $""{nameof(parameterName)} cannot be null"");
            if (!condition)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    throw new ArgumentException(message, parameterName);
                }
                else
                {
                    throw new ArgumentException(parameterName);
                }
            }
        }
    }
}";

            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Windows.Markup;

    /// <summary>
    /// Markupextension for getting Enum.GetValues(this.Type)
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
            Ensure.IsTrue(type.IsEnum, nameof(type), ""Expected type to be an enum"");
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
                Ensure.NotNull(value, nameof(value));
                Ensure.IsTrue(value.IsEnum, nameof(value), $""Expected {this.Type} to be an enum."");
                this.type = value;
            }
        }

        /// <inheritdoc/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            Ensure.NotNull(this.Type, nameof(this.Type));
            return Enum.GetValues(this.Type);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, ensureCode, testCode);
        }
    }
}
