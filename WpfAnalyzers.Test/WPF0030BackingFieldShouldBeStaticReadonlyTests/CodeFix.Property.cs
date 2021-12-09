namespace WpfAnalyzers.Test.WPF0030BackingFieldShouldBeStaticReadonlyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class Property
        {
            private static readonly DependencyPropertyBackingFieldOrPropertyAnalyzer Analyzer = new();
            private static readonly MakePropertyStaticReadonlyFix Fix = new();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0030BackingFieldShouldBeStaticReadonly);

            [Test]
            public static void DependencyPropertyRegisterBackingPropertyGetOnlyNotStatic()
            {
                var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public DependencyProperty ↓BarProperty { get; } = DependencyProperty.Register(nameof(Bar), typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";

                var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static DependencyProperty BarProperty { get; } = DependencyProperty.Register(nameof(Bar), typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void DependencyPropertyRegisterBackingPropertyGetSet()
            {
                var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static DependencyProperty ↓BarProperty { get; set; } = DependencyProperty.Register(
            ""Bar"", typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";

                var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static DependencyProperty BarProperty { get; } = DependencyProperty.Register(
            ""Bar"", typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }

            [Test]
            public static void DependencyPropertyRegisterBackingPropertyExpressionBody()
            {
                var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static DependencyProperty ↓BarProperty => DependencyProperty.Register(
            ""Bar"", typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";

                var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static DependencyProperty BarProperty { get; } = DependencyProperty.Register(
            ""Bar"", typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }
        }
    }
}
