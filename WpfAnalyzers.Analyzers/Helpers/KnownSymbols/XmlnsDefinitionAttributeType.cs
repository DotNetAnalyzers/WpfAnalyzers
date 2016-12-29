namespace WpfAnalyzers
{
    internal class XmlnsDefinitionAttributeType : QualifiedType
    {
        internal readonly string XmlNamespaceArgumentName = "xmlNamespace";
        internal readonly string ClrNamespaceArgumentName = "clrNamespace";

        internal readonly QualifiedProperty AssemblyName;

        public XmlnsDefinitionAttributeType()
            : base("System.Windows.Markup.XmlnsDefinitionAttribute")
        {
            this.AssemblyName = new QualifiedProperty(this, nameof(this.AssemblyName));
        }
    }
}