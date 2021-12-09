namespace WpfAnalyzers.Test.WPF0085XamlSetTypeConverterTargetTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Diagnostics
    {
        private static readonly AttributeAnalyzer Analyzer = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0085XamlSetTypeConverterTarget);

        [Test]
        public static void Message()
        {
            var code = @"
namespace N
{
    using System.Windows.Controls;
    using System.Windows.Markup;

    [XamlSetTypeConverter(nameof(↓ReceiveTypeConverter))]
    public class FooControl : Control
    {
        public static int ReceiveTypeConverter(object targetObject, XamlSetTypeConverterEventArgs eventArgs) => 1;
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Expected a method with signature void ReceiveTypeConverter(object, XamlSetTypeConverterEventArgs)"), code);
        }

        [Test]
        public static void WhenReturningInt()
        {
            var code = @"
namespace N
{
    using System.Windows.Controls;
    using System.Windows.Markup;

    [XamlSetTypeConverter(nameof(↓ReceiveTypeConverter))]
    public class FooControl : Control
    {
        public static int ReceiveTypeConverter(object targetObject, XamlSetTypeConverterEventArgs eventArgs) => 1;
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void WhenNoParameters()
        {
            var code = @"
namespace N
{
    using System.Windows.Controls;
    using System.Windows.Markup;

    [XamlSetTypeConverter(nameof(↓ReceiveTypeConverter))]
    public class FooControl : Control
    {
        public static void ReceiveTypeConverter()
        {
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void WhenMOreThanTwoParameters()
        {
            var code = @"
namespace N
{
    using System.Windows.Controls;
    using System.Windows.Markup;

    [XamlSetTypeConverter(nameof(↓ReceiveTypeConverter))]
    public class FooControl : Control
    {
        public static void ReceiveTypeConverter(object targetObject, XamlSetTypeConverterEventArgs eventArgs, int i)
        {
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void WhenFirstParameterIsInt()
        {
            var code = @"
namespace N
{
    using System.Windows.Controls;
    using System.Windows.Markup;

    [XamlSetTypeConverter(nameof(↓ReceiveTypeConverter))]
    public class FooControl : Control
    {
        public static void ReceiveTypeConverter(int targetObject, XamlSetTypeConverterEventArgs eventArgs)
        {
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }

        [Test]
        public static void WhenSecondParameterIsInt()
        {
            var code = @"
namespace N
{
    using System.Windows.Controls;
    using System.Windows.Markup;

    [XamlSetTypeConverter(nameof(↓ReceiveTypeConverter))]
    public class FooControl : Control
    {
        public static void ReceiveTypeConverter(object targetObject, int eventArgs)
        {
        }
    }
}";

            RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
        }
    }
}
