namespace WpfAnalyzers.Test.WPF0102EventDeclarationNameTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class HappyPath
    {
        private static readonly RoutedEventEventDeclarationAnalyzer Analyzer = new RoutedEventEventDeclarationAnalyzer();

        [TestCase("\"ValueChanged\"")]
        [TestCase("nameof(ValueChanged)")]
        [TestCase("nameof(FooControl.ValueChanged)")]
        public void EventManagerRegisterRoutedEvent(string nameof)
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the ValueChanged event</summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(FooControl));

        public event RoutedEventHandler ValueChanged
        {
            add { this.AddHandler(ValueChangedEvent, value); }
            remove { this.RemoveHandler(ValueChangedEvent, value); }
        }
    }
}";
            testCode = testCode.AssertReplace("nameof(ValueChanged)", nameof);
            AnalyzerAssert.Valid(Analyzer, testCode);
        }
    }
}
