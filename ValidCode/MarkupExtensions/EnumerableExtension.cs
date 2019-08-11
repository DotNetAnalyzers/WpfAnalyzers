namespace ValidCode.MarkupExtensions
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(IEnumerable))]
    public class EnumerableExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enumerable.Empty<int>();
        }
    }
}