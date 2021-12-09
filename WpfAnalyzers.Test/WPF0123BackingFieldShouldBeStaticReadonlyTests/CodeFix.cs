namespace WpfAnalyzers.Test.WPF0123BackingFieldShouldBeStaticReadonlyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class CodeFix
    {
        private static readonly RoutedCommandCreationAnalyzer Analyzer = new();
        private static readonly MakeFieldStaticReadonlyFix FieldFix = new();
        private static readonly MakePropertyStaticReadonlyFix PropertyFix = new();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0123BackingMemberShouldBeStaticReadonly);

        [Test]
        public static void RoutedCommandNotReadonlyField()
        {
            var before = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public static RoutedCommand ↓Bar = new RoutedCommand(nameof(Bar), typeof(Foo));
    }
}";

            var after = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public static readonly RoutedCommand Bar = new RoutedCommand(nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, FieldFix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void RoutedCommandNotStaticField()
        {
            var before = @"
namespace N
{
    using System.Windows.Input;

    public class Foo
    {
        public readonly RoutedCommand ↓Bar = new RoutedCommand(nameof(Bar), typeof(Foo));
    }
}";

            var after = @"
namespace N
{
    using System.Windows.Input;

    public class Foo
    {
        public static readonly RoutedCommand Bar = new RoutedCommand(nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, FieldFix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void RoutedCommandMutableField()
        {
            var before = @"
namespace N
{
    using System.Windows.Input;

    public class Foo
    {
        public RoutedCommand ↓Bar = new RoutedCommand(nameof(Bar), typeof(Foo));
    }
}";

            var after = @"
namespace N
{
    using System.Windows.Input;

    public class Foo
    {
        public static readonly RoutedCommand Bar = new RoutedCommand(nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, FieldFix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void RoutedUICommandStaticMutableProperty()
        {
            var before = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public static RoutedUICommand ↓Bar { get; set; } = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";

            var after = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public static RoutedUICommand Bar { get; } = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, PropertyFix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void RoutedUICommandStaticExpressionBody()
        {
            var before = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public static RoutedUICommand ↓Bar => new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";

            var after = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public static RoutedUICommand Bar { get; } = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, PropertyFix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void RoutedUICommandInstanceExpressionBody()
        {
            var before = @"
namespace N
{
    using System.Windows.Input;

    public class Foo
    {
        public RoutedUICommand ↓Bar => new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";

            var after = @"
namespace N
{
    using System.Windows.Input;

    public class Foo
    {
        public static RoutedUICommand Bar { get; } = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, PropertyFix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void RoutedUICommandInstanceProperty()
        {
            var before = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public RoutedUICommand ↓Bar { get; } = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";

            var after = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public static RoutedUICommand Bar { get; } = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, PropertyFix, ExpectedDiagnostic, before, after);
        }

        [Test]
        public static void RoutedUICommandMutableInstanceProperty()
        {
            var before = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public RoutedUICommand ↓Bar { get; set; } = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";

            var after = @"
namespace N
{
    using System.Windows.Input;

    public static class Foo
    {
        public static RoutedUICommand Bar { get; } = new RoutedUICommand(""Some text"", nameof(Bar), typeof(Foo));
    }
}";
            RoslynAssert.CodeFix(Analyzer, PropertyFix, ExpectedDiagnostic, before, after);
        }
    }
}
