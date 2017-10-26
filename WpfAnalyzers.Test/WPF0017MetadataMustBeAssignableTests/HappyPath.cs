namespace WpfAnalyzers.Test.WPF0017MetadataMustBeAssignableTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class HappyPath
    {
        private static readonly WPF0017MetadataMustBeAssignable Analyzer = new WPF0017MetadataMustBeAssignable();

        [TestCase("new PropertyMetadata(1)")]
        [TestCase("new FrameworkPropertyMetadata(default(int))")]
        public void OverrideMetadataWhenBaseHasNone(string metadata)
        {
            var fooControlCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(FooControl));

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }
    }
}";

            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class BarControl : FooControl
    {
        static BarControl()
        {
            ValueProperty.OverrideMetadata(typeof(BarControl), new PropertyMetadata(1));
        }
    }
}";
            testCode = testCode.AssertReplace("new PropertyMetadata(1)", metadata);
            AnalyzerAssert.Valid(Analyzer, fooControlCode, testCode);
        }

        [TestCase("new PropertyMetadata(1)")]
        [TestCase("new FrameworkPropertyMetadata(default(int))")]
        public void OverrideMetadataWithSameType(string metadata)
        {
            var fooControlCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }
    }
}";

            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class BarControl : FooControl
    {
        static BarControl()
        {
            ValueProperty.OverrideMetadata(typeof(BarControl), new PropertyMetadata(1));
        }
    }
}";
            testCode = testCode.AssertReplace("new PropertyMetadata(1)", metadata);
            AnalyzerAssert.Valid(Analyzer, fooControlCode, testCode);
        }
    }
}