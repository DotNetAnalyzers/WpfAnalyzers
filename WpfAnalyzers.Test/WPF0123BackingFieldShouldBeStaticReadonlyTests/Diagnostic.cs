namespace WpfAnalyzers.Test.WPF0123BackingFieldShouldBeStaticReadonlyTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly DiagnosticAnalyzer Analyzer = new RoutedCommandCreationAnalyzer();
        private static readonly CodeFixProvider FieldFix = new MakeFieldStaticReadonlyFix();
        private static readonly CodeFixProvider PropertyFix = new MakePropertyStaticReadonlyFix();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(WPF0123BackingMemberShouldBeStaticReadonly.Descriptor);

        [Test]
        public static void RoutedCommandNotReadonlyField()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public static class Foo
    {
        public static RoutedCommand ↓Bar = new RoutedCommand(nameof(Bar), typeof(Foo));
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public static class Foo
    {
        public static readonly RoutedCommand Bar = new RoutedCommand(nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, FieldFix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public static void RoutedCommandNotStaticField()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public class Foo
    {
        public readonly RoutedCommand ↓Bar = new RoutedCommand(nameof(Bar), typeof(Foo));
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public class Foo
    {
        public static readonly RoutedCommand Bar = new RoutedCommand(nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, FieldFix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public static void RoutedCommandMutableField()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public class Foo
    {
        public RoutedCommand ↓Bar = new RoutedCommand(nameof(Bar), typeof(Foo));
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public class Foo
    {
        public static readonly RoutedCommand Bar = new RoutedCommand(nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, FieldFix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public static void RoutedUICommandStaticMutableProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public static class Foo
    {
        public static RoutedUICommand ↓Bar { get; set; } = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public static class Foo
    {
        public static RoutedUICommand Bar { get; } = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, PropertyFix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public static void RoutedUICommandStaticExpressionBody()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public static class Foo
    {
        public static RoutedUICommand ↓Bar => new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public static class Foo
    {
        public static RoutedUICommand Bar { get; } = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, PropertyFix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public static void RoutedUICommandInstanceExpressionBody()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public class Foo
    {
        public RoutedUICommand ↓Bar => new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public class Foo
    {
        public static RoutedUICommand Bar { get; } = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, PropertyFix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public static void RoutedUICommandInstanceProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public static class Foo
    {
        public RoutedUICommand ↓Bar { get; } = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public static class Foo
    {
        public static RoutedUICommand Bar { get; } = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, PropertyFix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public static void RoutedUICommandMutableInstanceProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public static class Foo
    {
        public RoutedUICommand ↓Bar { get; set; } = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.Windows.Input;

    public static class Foo
    {
        public static RoutedUICommand Bar { get; } = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, PropertyFix, ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}
