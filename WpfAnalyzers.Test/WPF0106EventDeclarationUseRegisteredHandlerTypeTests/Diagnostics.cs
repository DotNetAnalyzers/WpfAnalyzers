namespace WpfAnalyzers.Test.WPF0106EventDeclarationUseRegisteredHandlerTypeTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal class Diagnostics
    {
        [Test]
        public void Message()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public class FooControl : Control
    {
        /// <summary>Identifies the ValueChanged event</summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            ""ValueChanged"",
            RoutingStrategy.Direct,
            typeof(RoutedEventHandler),
            typeof(FooControl));

        public event ↓EventHandler ValueChanged
        {
            add { this.AddHandler(ValueChangedEvent, value); }
            remove { this.AddHandler(ValueChangedEvent, value); }
        }
    }
}";

            var expectedDiagnostic = ExpectedDiagnostic.Create(
                "WPF0106",
                "Use the registered handler type RoutedEventHandler.");
            AnalyzerAssert.Diagnostics<RoutedEventEventDeclarationAnalyzer>(expectedDiagnostic, testCode);
        }
    }
}
