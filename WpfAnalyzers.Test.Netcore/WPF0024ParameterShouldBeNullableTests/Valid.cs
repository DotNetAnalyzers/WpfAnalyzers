﻿namespace WpfAnalyzers.Test.Netcore.WPF0024ParameterShouldBeNullableTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly DiagnosticAnalyzer Analyzer = new PropertyMetadataAnalyzer();

        [Test]
        public static void Nullable()
        {
            var code = @"
#nullable enable
namespace N
{
    using System.Windows;

    public class C : FrameworkElement
    {
        /// <summary>Identifies the <see cref=""Text""/> dependency property.</summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(C),
            new PropertyMetadata(
                string.Empty,
                null,
                (d, o) => CoerceText(d, o)));

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        private static object CoerceText(DependencyObject d, object? o)
        {
            _ = d;
            return o switch
            {
                null => string.Empty,
                string s => s,
                _ => o.ToString() ?? ""null"",
            };
        }
    }
}
";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public static void NotNullable()
        {
            var code = @"
namespace N
{
    using System.Windows;

    public class C : FrameworkElement
    {
        /// <summary>Identifies the <see cref=""Text""/> dependency property.</summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(C),
            new PropertyMetadata(
                string.Empty,
                null,
                (d, o) => CoerceText(d, o)));

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        private static object CoerceText(DependencyObject d, object o)
        {
            _ = d;
            return o switch
            {
                null => string.Empty,
                string s => s,
                _ => o.ToString() ?? ""null"",
            };
        }
    }
}
";
            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
