namespace WpfAnalyzers.Test.WPF0032ClrPropertyGetAndSetSameDependencyPropertyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class Diagnostics
    {
        [Test]
        public void DependencyProperty()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
            nameof(Bar),
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        public static readonly DependencyProperty OtherProperty = DependencyProperty.Register(
            ""Other"",
            typeof(int),
            typeof(FooControl),
            new PropertyMetadata(default(int)));

        ↓public int Bar
        {
            get { return (int)this.GetValue(BarProperty); }
            set { this.SetValue(OtherProperty, value); }
        }
    }
}";
            AnalyzerAssert.Diagnostics<WPF0032ClrPropertyGetAndSetSameDependencyProperty>(testCode);
        }
    }
}