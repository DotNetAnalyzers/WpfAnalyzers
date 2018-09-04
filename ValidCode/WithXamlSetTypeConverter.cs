namespace ValidCode
{
    using System.Windows.Controls;
    using System.Windows.Markup;

    [XamlSetTypeConverter(nameof(ReceiveTypeConverter))]
    public class WithXamlSetTypeConverter : Control
    {
        public static void ReceiveTypeConverter(object targetObject, XamlSetTypeConverterEventArgs eventArgs)
        {
        }
    }
}