namespace WpfAnalyzers.Test.WPF0105EventDeclarationRemoveHandlerInRemoveTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly RoutedEventEventDeclarationAnalyzer Analyzer = new();

    [TestCase("\"ValueChanged\"")]
    [TestCase("nameof(ValueChanged)")]
    [TestCase("nameof(FooControl.ValueChanged)")]
    public static void EventManagerRegisterRoutedEvent(string nameof)
    {
        var code = @"
namespace N
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
}".AssertReplace("nameof(ValueChanged)", nameof);

        RoslynAssert.Valid(Analyzer, code);
    }
}
