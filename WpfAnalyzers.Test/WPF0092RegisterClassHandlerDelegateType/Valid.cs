namespace WpfAnalyzers.Test.WPF0092RegisterClassHandlerDelegateType;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly RoutedEventCallbackAnalyzer Analyzer = new();

    [TestCase("new RoutedEventHandler(OnPasswordChanged)")]
    [TestCase("new RoutedEventHandler((sender, e) => { })")]
    [TestCase("new RoutedEventHandler((sender, e) => OnPasswordChanged(sender, e))")]
    public static void RegisterClassHandlerPasswordChangedEvent(string expression)
    {
        var code = @"
namespace N;

using System.Windows;
using System.Windows.Controls;

public static class C
{
    static C()
    {
        EventManager.RegisterClassHandler(
            typeof(PasswordBox),
            PasswordBox.PasswordChangedEvent,
            new RoutedEventHandler(OnPasswordChanged));

#pragma warning disable CS8321
        static void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
        }
    }
}".AssertReplace("new RoutedEventHandler(OnPasswordChanged)", expression);
        RoslynAssert.Valid(Analyzer, code);
    }

    [TestCase("new KeyEventHandler(OnKeyDown)")]
    [TestCase("new KeyEventHandler((sender, e) => { })")]
    [TestCase("new KeyEventHandler((sender, e) => OnKeyDown(sender, e))")]
    public static void RegisterClassHandlerKeyDownEvent(string expression)
    {
        var code = @"
namespace N;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public static class C
{
    static C()
    {
        EventManager.RegisterClassHandler(
            typeof(TextBox),
            TextBox.KeyDownEvent,
            new KeyEventHandler(OnKeyDown));

#pragma warning disable CS8321
        static void OnKeyDown(object sender, KeyEventArgs e)
        {
        }
    }
}".AssertReplace("new KeyEventHandler(OnKeyDown)", expression);
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void EventDeclaration()
    {
        var code = @"
namespace N;

using System.Windows;
using System.Windows.Controls;

public class FooControl : Control
{
    /// <summary>Identifies the <see cref=""ValueChanged""/> routed event.</summary>
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
}";
        RoslynAssert.Valid(Analyzer, code);
    }

    [Test]
    public static void RoutedEventHelper()
    {
        var code = @"
namespace ValidCode.RoutedEvents;

using System;
using System.Windows;

internal static class RoutedEventHelper
{
    internal static void UpdateHandler(this UIElement element, RoutedEvent routedEvent, Delegate handler)
    {
        element.RemoveHandler(routedEvent, handler);
        element.AddHandler(routedEvent, handler);
    }

    internal static void UpdateHandler(this UIElement element, RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
    {
        element.RemoveHandler(routedEvent, handler);
        element.AddHandler(routedEvent, handler, handledEventsToo);
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }
}
