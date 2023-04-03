namespace ValidCode.MarkupExtensions;

using System;
using System.Windows.Markup;

[MarkupExtensionReturnType(typeof(IdExtension))]
public class IdExtension : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
