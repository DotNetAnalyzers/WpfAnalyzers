namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0051XmlnsDefinitionMustMapExistingNamespace
    {
        public const string DiagnosticId = "WPF0051";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "XmlnsDefinition must map to existing namespace.",
            messageFormat: "[XmlnsDefinition] maps to '{0}' that does not exist.",
            category: AnalyzerCategory.XmlnsDefinition,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "XmlnsDefinition must map to existing namespace.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
