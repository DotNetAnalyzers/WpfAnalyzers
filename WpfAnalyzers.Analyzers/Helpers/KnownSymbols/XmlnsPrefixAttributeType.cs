namespace WpfAnalyzers
{
    internal class XmlnsPrefixAttributeType : QualifiedType
    {
        internal readonly string XmlNamespaceArgumentName = "xmlNamespace";
        internal readonly string PrefixArgumentName = "prefix";

        public XmlnsPrefixAttributeType()
            : base("System.Windows.Markup.XmlnsPrefixAttribute")
        {
        }
    }
}