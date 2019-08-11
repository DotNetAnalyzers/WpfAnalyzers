namespace ValidCode.MarkupExtensions
{
    using System;
    using System.Windows.Markup;

    [MarkupExtensionReturnType(typeof(MarkupExtension))]
    public class Id2Extension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}