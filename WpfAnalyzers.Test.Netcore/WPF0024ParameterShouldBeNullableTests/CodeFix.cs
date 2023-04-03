namespace WpfAnalyzers.Test.Netcore.WPF0024ParameterShouldBeNullableTests;

using Gu.Roslyn.Asserts;

using NUnit.Framework;

public static class CodeFix
{
    private static readonly PropertyMetadataAnalyzer PropertyMetadataAnalyzer = new();
    private static readonly RegistrationAnalyzer RegistrationAnalyzer = new();
    private static readonly MakeNullableFix Fix = new();
    private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create(Descriptors.WPF0024ParameterShouldBeNullable);

    [Test]
    public static void NullableCoerce()
    {
        var before = @"
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

        private static object CoerceText(DependencyObject d, ↓object o)
        {
            return o switch
            {
                null => string.Empty,
                string s => s,
                _ => o.ToString() ?? ""null"",
            };
        }
    }
}";

        var after = @"
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
            return o switch
            {
                null => string.Empty,
                string s => s,
                _ => o.ToString() ?? ""null"",
            };
        }
    }
}";
        RoslynAssert.CodeFix(PropertyMetadataAnalyzer, Fix, ExpectedDiagnostic, before, after);
    }

    [Test]
    public static void NullableValidate()
    {
        var before = @"
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
            new PropertyMetadata(string.Empty),
            (o) => ValidateText(o));

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        private static bool ValidateText(↓object o)
        {
            return o switch
            {
                null => false,
                string s => true,
                _ => o.ToString() is null ? false : true,
            };
        }
    }
}";

        var after = @"
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
            new PropertyMetadata(string.Empty),
            (o) => ValidateText(o));

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        private static bool ValidateText(object? o)
        {
            return o switch
            {
                null => false,
                string s => true,
                _ => o.ToString() is null ? false : true,
            };
        }
    }
}";
        RoslynAssert.CodeFix(RegistrationAnalyzer, Fix, ExpectedDiagnostic, before, after);
    }
}
