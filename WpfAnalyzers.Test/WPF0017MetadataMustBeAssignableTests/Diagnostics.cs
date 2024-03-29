﻿namespace WpfAnalyzers.Test.WPF0017MetadataMustBeAssignableTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Diagnostics
{
    private static readonly OverrideMetadataAnalyzer Analyzer = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0017MetadataMustBeAssignable);

    [Test]
    public static void DependencyPropertyOverrideMetadataWithBaseType()
    {
        var fooControlCode = @"
namespace N
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
namespace N
{
    using System.Windows;

    public class BarControl : FooControl
    {
        static BarControl()
        {
            ValueProperty.OverrideMetadata(typeof(BarControl), ↓new PropertyMetadata(1));
        }
    }
}";

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, fooControlCode, barControlCode);
    }

    [Test]
    public static void DependencyPropertyOverrideMetadataWithBaseTypeFullyQualified()
    {
        var fooControlCode = @"
namespace N
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
namespace N
{
    using System.Windows;

    public class BarControl : FooControl
    {
        static BarControl()
        {
            FooControl.ValueProperty.OverrideMetadata(typeof(BarControl), ↓new PropertyMetadata(1));
        }
    }
}";

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, fooControlCode, barControlCode);
    }

    [Test]
    public static void DefaultStyleKeyPropertyOverrideMetadata()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        static FooControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FooControl), ↓new PropertyMetadata(typeof(FooControl)));
        }
    }
}";

        RoslynAssert.Diagnostics(Analyzer, ExpectedDiagnostic, code);
    }
}
