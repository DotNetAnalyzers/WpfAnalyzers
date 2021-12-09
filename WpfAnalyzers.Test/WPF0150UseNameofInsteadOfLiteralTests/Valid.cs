namespace WpfAnalyzers.Test.WPF0150UseNameofInsteadOfLiteralTests
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

            RoslynAssert.Valid(new RegistrationAnalyzer(), code);
        }

        [Test]
        public static void DependencyPropertyRegisterWhenNameof()
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Bar
        {
            get => (int)GetValue(BarProperty);
            set => SetValue(BarProperty, value);
        }
    }
}";

            RoslynAssert.Valid(new RegistrationAnalyzer(), code);
        }
    }
}
