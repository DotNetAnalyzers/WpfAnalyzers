// ReSharper disable All
namespace ValidCode.Recursion;

using System;

[System.Windows.Markup.MarkupExtensionReturnType(typeof(object))]
public class RecursiveExtension : System.Windows.Markup.MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this.ProvideValue(serviceProvider);
}
