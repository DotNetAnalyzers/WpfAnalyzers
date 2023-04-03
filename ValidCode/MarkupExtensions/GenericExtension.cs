// ReSharper disable UnusedMember.Global
namespace ValidCode.MarkupExtensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;

[MarkupExtensionReturnType(typeof(IEnumerable<int>))]
public class GenericExtension : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return Enumerable.Empty<int>();
    }
}
