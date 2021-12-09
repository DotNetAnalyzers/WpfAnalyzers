namespace WpfAnalyzers.Test.WPF0030BackingFieldShouldBeStaticReadonlyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class FixAll
        {
            private static readonly DependencyPropertyBackingFieldOrPropertyAnalyzer Analyzer = new();
            private static readonly MakeFieldStaticReadonlyFix Fix = new();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0030BackingFieldShouldBeStaticReadonly);

            [TestCase("public static", "public static readonly")]
            [TestCase("public", "public static readonly")]
            [TestCase("public readonly", "public static readonly")]
            [TestCase("private static", "private static readonly")]
            [TestCase("private", "private static readonly")]
            public static void DependencyPropertyRegisterBackingField(string modifiersBefore, string modifiersAfter)
            {
                var before = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static DependencyProperty ↓BarProperty = DependencyProperty.Register(nameof(Bar), typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}".AssertReplace("public static DependencyProperty", modifiersBefore + " DependencyProperty");

                var after = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(nameof(Bar), typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}".AssertReplace("public static readonly DependencyProperty", modifiersAfter + " DependencyProperty");

                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, before, after);
            }
        }
    }
}
