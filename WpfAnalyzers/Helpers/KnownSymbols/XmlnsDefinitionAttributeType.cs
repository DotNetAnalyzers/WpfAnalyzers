namespace WpfAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;

    internal class XmlnsDefinitionAttributeType : QualifiedType
    {
        internal readonly string XmlNamespaceArgumentName = "xmlNamespace";
        internal readonly string ClrNamespaceArgumentName = "clrNamespace";

        internal readonly QualifiedProperty AssemblyName;

        internal XmlnsDefinitionAttributeType()
            : base("System.Windows.Markup.XmlnsDefinitionAttribute")
        {
            this.AssemblyName = new QualifiedProperty(this, nameof(this.AssemblyName));
        }
    }
}
