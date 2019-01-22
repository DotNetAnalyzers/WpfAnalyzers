namespace WpfAnalyzers.Test.WPF0084XamlSetMarkupExtensionAttributeTargetTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    internal class Diagnostics
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0084XamlSetMarkupExtensionAttributeTarget.Descriptor);

        [Test]
        public void Message()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Controls;
    using System.Windows.Markup;

    [XamlSetMarkupExtension(↓nameof(ReceiveMarkupExtension))]
    public class WithSetMarkupExtensionAttribute : Control
    {
        public static int ReceiveMarkupExtension(object targetObject, XamlSetMarkupExtensionEventArgs eventArgs)
        {
            return 1;
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic.WithMessage("Expected a method with signature void ReceiveMarkupExtension(object, XamlSetMarkupExtensionEventArgs)."), testCode);
        }

        [Test]
        public void WhenReturningInt()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Controls;
    using System.Windows.Markup;

    [XamlSetMarkupExtension(↓nameof(ReceiveMarkupExtension))]
    public class WithSetMarkupExtensionAttribute : Control
    {
        public static int ReceiveMarkupExtension(object targetObject, XamlSetMarkupExtensionEventArgs eventArgs)
        {
            return 1;
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [Test]
        public void WhenNoParameters()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Controls;
    using System.Windows.Markup;

    [XamlSetMarkupExtension(↓nameof(ReceiveMarkupExtension))]
    public class WithSetMarkupExtensionAttribute : Control
    {
        public static void ReceiveMarkupExtension()
        {
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [Test]
        public void WhenMOreThanTwoParameters()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Controls;
    using System.Windows.Markup;

    [XamlSetMarkupExtension(↓nameof(ReceiveMarkupExtension))]
    public class WithSetMarkupExtensionAttribute : Control
    {
        public static void ReceiveMarkupExtension(object targetObject, XamlSetMarkupExtensionEventArgs eventArgs, int i)
        {
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [Test]
        public void WhenFirstParameterIsInt()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Controls;
    using System.Windows.Markup;

    [XamlSetMarkupExtension(↓nameof(ReceiveMarkupExtension))]
    public class WithSetMarkupExtensionAttribute : Control
    {
        public static void ReceiveMarkupExtension(int targetObject, XamlSetMarkupExtensionEventArgs eventArgs)
        {
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }

        [Test]
        public void WhenSecondParameterIsInt()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Controls;
    using System.Windows.Markup;

    [XamlSetMarkupExtension(↓nameof(ReceiveMarkupExtension))]
    public class WithSetMarkupExtensionAttribute : Control
    {
        public static void ReceiveMarkupExtension(object targetObject, int eventArgs)
        {
        }
    }
}";

            AnalyzerAssert.Diagnostics(Analyzer, ExpectedDiagnostic, testCode);
        }
    }
}
