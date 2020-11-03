namespace WpfAnalyzers.Test.Netcore.WPF0012ClrPropertyShouldMatchRegisteredTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly ClrPropertyDeclarationAnalyzer Analyzer = new ClrPropertyDeclarationAnalyzer();

        [TestCase("default(string)")]
        [TestCase("(object)null")]
        public static void DependencyProperty(string typeName)
        {
            var code = @"
#nullable enable
namespace N
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar), 
            typeof(string?), 
            typeof(FooControl),
            new PropertyMetadata(default(string)));

        public string? Bar
        {
            get => (string?)GetValue(BarProperty);
            set => SetValue(BarProperty, value);
        }
    }
}".AssertReplace("default(string)", typeName);

            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void DependencyPropertyWithThis()
        {
            var code = @"
#nullable enable
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar), 
            typeof(string?), 
            typeof(FooControl),
            new PropertyMetadata(default(string)));

        public string? Bar
        {
            get => (string?)GetValue(BarProperty);
            set => SetValue(BarProperty, value);
        }
    }
}";

            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
