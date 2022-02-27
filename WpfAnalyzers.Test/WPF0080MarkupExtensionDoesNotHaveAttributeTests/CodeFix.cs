namespace WpfAnalyzers.Test.WPF0080MarkupExtensionDoesNotHaveAttributeTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class CodeFix
{
    private static readonly WPF0080MarkupExtensionDoesNotHaveAttribute Analyzer = new();
    private static readonly MarkupExtensionReturnTypeAttributeFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0080MarkupExtensionDoesNotHaveAttribute);

    [Test]
    public static void Message()
    {
        var code = @"
namespace N
{
    using System;
    using System.Windows.Markup;

    public class ↓FooExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}";

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Add MarkupExtensionReturnType attribute"), code);
    }

    [Test]
    public static void ReturnsThis()
    {
        var before = @"
namespace N
{
    using System;
    using System.Windows.Markup;

    public class ↓FooExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}";

        var after = @"
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
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void WhenDocumented()
    {
        var before = @"
namespace N
{
    using System;
    using System.Windows.Markup;

    /// <summary>
    /// Text
    /// </summary>
    public class ↓FooExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}";

        var after = @"
namespace N
{
    using System;
    using System.Windows.Markup;

    /// <summary>
    /// Text
    /// </summary>
    [MarkupExtensionReturnType(typeof(FooExtension))]
    public class FooExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}";
        RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
    }
}
