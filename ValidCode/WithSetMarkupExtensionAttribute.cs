namespace ValidCode;

using System.Windows.Controls;
using System.Windows.Markup;

[XamlSetMarkupExtension(nameof(ReceiveMarkupExtension))]
public class WithSetMarkupExtensionAttribute : Control
{
    public static void ReceiveMarkupExtension(object targetObject, XamlSetMarkupExtensionEventArgs eventArgs)
    {
    }
}
