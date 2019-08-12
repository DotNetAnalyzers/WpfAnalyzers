namespace WpfAnalyzers.Test.WPF0150UseNameofTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Valid
    {
        [Test]
        public static void DependencyPropertyRegisterWhenNoProperty()
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            ""Bar"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));
    }
}";

            RoslynAssert.Valid(new DependencyPropertyBackingFieldOrPropertyAnalyzer(), code);
        }
    }
}
