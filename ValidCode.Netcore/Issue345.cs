namespace ValidCode.Netcore;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public static class Issue345
{
    static Issue345()
    {
        EventManager.RegisterClassHandler(typeof(TreeViewItem), UIElement.MouseLeftButtonDownEvent,  new MouseButtonEventHandler(OnMouseButtonDown));
        EventManager.RegisterClassHandler(typeof(TreeViewItem), UIElement.MouseRightButtonDownEvent, new MouseButtonEventHandler(OnMouseButtonDown));

        EventManager.RegisterClassHandler(typeof(DataGridRow), UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnDown));
        EventManager.RegisterClassHandler(typeof(DataGridRow), UIElement.TouchDownEvent,           new EventHandler<TouchEventArgs>(OnDown));

        static void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        static void OnDown(object? sender, EventArgs e)
        {
        }
    }
}
