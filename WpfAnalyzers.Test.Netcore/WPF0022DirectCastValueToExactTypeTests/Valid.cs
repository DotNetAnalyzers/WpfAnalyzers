namespace WpfAnalyzers.Test.Netcore.WPF0022DirectCastValueToExactTypeTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly CallbackAnalyzer Analyzer = new();

    [Test]
    public static void NullableStringControl()
    {
        var code = @"
namespace N
{
    using System.Windows;
    using System.Windows.Controls;

    public class NullableStringControl : Control
    {
        /// <summary>Identifies the <see cref=""Text""/> dependency property.</summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(NullableStringControl),
            new PropertyMetadata(
                null,
                (d, e) => ((NullableStringControl)d).OnTextChanged((string?)e.NewValue, (string?)e.OldValue)));

        static NullableStringControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NullableStringControl), new FrameworkPropertyMetadata(typeof(NullableStringControl)));
        }

        public string? Text
        {
            get => (string?)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        protected void OnTextChanged(string? oldValue, string? newValue)
        {
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }
}
