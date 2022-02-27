﻿namespace WpfAnalyzers.Test.WPF0002BackingFieldShouldMatchRegisteredNameTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly DependencyPropertyBackingFieldOrPropertyAnalyzer Analyzer = new();

    [TestCase("\"Bar\"")]
    [TestCase("nameof(Bar)")]
    [TestCase("nameof(FooControl.Bar)")]
    public static void DependencyPropertyRegisterReadOnlyBackingFields(string nameof)
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;
    
        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarPropertyKey, value); }
        }
    }
}".AssertReplace("nameof(Bar)", nameof);
        RoslynAssert.Valid(Analyzer, code);
    }

    [TestCase("\"Bar\"")]
    [TestCase("nameof(Bar)")]
    [TestCase("nameof(FooControl.Bar)")]
    public static void DependencyPropertyRegisterReadOnlyBackingProperties(string nameof)
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        private static DependencyPropertyKey BarPropertyKey { get; } = DependencyProperty.RegisterReadOnly(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        /// <summary>Identifies the <see cref=""Bar""/> dependency property.</summary>
        public static DependencyProperty BarProperty { get; } = BarPropertyKey.DependencyProperty;
    
        public int Bar
        {
            get { return (int)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }
    }
}".AssertReplace("nameof(Bar)", nameof);
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void DependencyPropertyRegisterReadOnlyRepro()
    {
        var statusCode = @"
namespace N
{
    public enum Status
    {
        Idle,
        Updating
    }
}";
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the <see cref=""Status""/> dependency property.</summary>
        internal static readonly DependencyPropertyKey StatusPropertyKey = DependencyProperty.RegisterReadOnly(
            ""Status"",
            typeof(Status),
            typeof(FooControl),
            new PropertyMetadata(Status.Idle, OnStatusChanged));

        /// <summary>Identifies the <see cref=""Status""/> dependency property.</summary>
        internal static readonly DependencyProperty StatusProperty = StatusPropertyKey.DependencyProperty;

        internal Status Status
        {
            get { return (Status)this.GetValue(StatusProperty); }
            set { this.SetValue(StatusProperty, value); }
        }

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nop
        }
    }
}";
        RoslynAssert.Valid(Analyzer, statusCode, code);
    }

    [Test]
    public static void DependencyPropertyRegisterAttachedReadOnly()
    {
        var code = @"
namespace N
{
    using System.Windows;

    public static class Foo
    {
        private static readonly DependencyPropertyKey BarPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            ""Bar"",
            typeof(int),
            typeof(Foo),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty BarProperty = BarPropertyKey.DependencyProperty;

        public static void SetBar(DependencyObject element, int value)
        {
            element.SetValue(BarPropertyKey, value);
        }

        public static int GetBar(DependencyObject element)
        {
            return (int)element.GetValue(BarProperty);
        }
    }
}";

        RoslynAssert.Valid(Analyzer, code);
    }
}
