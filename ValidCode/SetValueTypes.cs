namespace ValidCode;

using System.Windows;
using System.Windows.Controls;

public class SetValueTypes : Control
{
    public static void M(SetValueTypes control)
    {
        control.Style = new Style(typeof(SetValueTypes));
        control.SetValue(FrameworkElement.StyleProperty, new Style(typeof(SetValueTypes)));
        control.DataContext = new object();
        control.SetValue(FrameworkElement.DataContextProperty, new object());
    }
}
