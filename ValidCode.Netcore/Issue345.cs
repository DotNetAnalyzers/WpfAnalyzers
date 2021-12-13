namespace ValidCode.Netcore;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public static class Issue345
{
    static Issue345()
    {
        EventManager.RegisterClassHandler(typeof(TreeViewItem), UIElement.MouseLeftButtonDownEvent,  new MouseButtonEventHandler(OnDown));
        EventManager.RegisterClassHandler(typeof(TreeViewItem), UIElement.MouseRightButtonDownEvent, new MouseButtonEventHandler(OnDown));

        static void OnDown(object sender, MouseButtonEventArgs e)
        {
        }
    }
}
