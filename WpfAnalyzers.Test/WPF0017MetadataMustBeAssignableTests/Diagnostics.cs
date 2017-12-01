namespace WpfAnalyzers.Test.WPF0017MetadataMustBeAssignableTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class Diagnostics
    {
        [Test]
        public void DependencyPropertyOverrideMetadataWithBaseType()
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
            new FrameworkPropertyMetadata(default(int)));

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }
    }
}";

            var barControlCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class BarControl : FooControl
    {
        static BarControl()
        {
            ValueProperty.OverrideMetadata(typeof(BarControl), ↓new PropertyMetadata(1));
        }
    }
}";

            AnalyzerAssert.Diagnostics<WPF0017MetadataMustBeAssignable>(fooControlCode, barControlCode);
        }

        [Test]
        public void DependencyPropertyOverrideMetadataWithBaseTypeFullyQualified()
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
            new FrameworkPropertyMetadata(default(int)));

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }
    }
}";

            var barControlCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class BarControl : FooControl
    {
        static BarControl()
        {
            FooControl.ValueProperty.OverrideMetadata(typeof(BarControl), ↓new PropertyMetadata(1));
        }
    }
}";

            AnalyzerAssert.Diagnostics<WPF0017MetadataMustBeAssignable>(fooControlCode, barControlCode);
        }
    }
}