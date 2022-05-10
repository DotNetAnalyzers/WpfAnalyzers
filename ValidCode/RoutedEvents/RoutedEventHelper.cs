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
}
