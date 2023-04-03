namespace WpfAnalyzers.Test.Netcore.WPF0024ParameterShouldBeNullableTests;

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
#nullable disable
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

    [Test]
    public static void CoerceValueCallback()
    {
        var code = @"
#pragma warning disable WPF0023
namespace N;

using System;
using System.Windows;

public class Chart : FrameworkElement
{
    /// <summary>Identifies the <see cref=""Time""/> dependency property.</summary>
    public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
        nameof(Time),
        typeof(DateTimeOffset),
        typeof(Chart),
        new FrameworkPropertyMetadata(
            default(DateTimeOffset),
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            propertyChangedCallback: null,
            coerceValueCallback: (_, o) => Min(DateTimeOffset.Now, (DateTimeOffset)o)));

    public DateTimeOffset Time
    {
        get => (DateTimeOffset)this.GetValue(TimeProperty);
        set => this.SetValue(TimeProperty, value);
    }
	
	private static DateTimeOffset Min(DateTimeOffset x, DateTimeOffset y) => x < y ? x : y;
}
";
        RoslynAssert.Valid(Analyzer, code);
    }
}
