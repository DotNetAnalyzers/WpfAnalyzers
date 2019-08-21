namespace WpfAnalyzers.Test.WPF0170StyleTypedPropertyPropertyTargetTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new AttributeAnalyzer();
        private static readonly DiagnosticDescriptor Descriptor = Descriptors.WPF0170StyleTypedPropertyPropertyTarget;

        [TestCase("[StyleTypedProperty(Property = nameof(BarStyle), StyleTargetType = typeof(Control))]")]
        [TestCase("[StyleTypedProperty(Property = \"BarStyle\", StyleTargetType = typeof(Control))]")]
        [TestCase("[StyleTypedProperty(Property = BarStyleName, StyleTargetType = typeof(Control))]")]
        public static void WhenExists(string attribute)
        {
            var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    [StyleTypedProperty(Property = nameof(BarStyle), StyleTargetType = typeof(Control))]
    public class WithStyleTypedProperty : Control
    {
        const string BarStyleName = nameof(BarStyle);

        /// <summary>Identifies the <see cref=""BarStyle""/> dependency property.</summary>
        public static readonly DependencyProperty BarStyleProperty = DependencyProperty.Register(
            nameof(BarStyle),
            typeof(Style),
            typeof(WithStyleTypedProperty),
            new PropertyMetadata(default(Style)));

        public Style BarStyle
        {
            get => (Style)this.GetValue(BarStyleProperty);
            set => this.SetValue(BarStyleProperty, value);
        }
    }
}".AssertReplace("[StyleTypedProperty(Property = nameof(BarStyle), StyleTargetType = typeof(Control))]", attribute);
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }

        [Test]
        public static void OtherAttributeAndAlias()
        {
            var code = @"
    using System;
    using Window = System.Windows.Window;

    class C
    {
        [Obsolete]
        void M(Window _)
        {
        }
    }";
            RoslynAssert.Valid(Analyzer, Descriptor, code);
        }
    }
}
