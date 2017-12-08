namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0034AttachedPropertyBrowsableForTypeAttributeArgument
    {
        public const string DiagnosticId = "WPF0034";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use correct argument for [AttachedPropertyBrowsableForType]",
            messageFormat: "Use [AttachedPropertyBrowsableForType(typeof({0})]",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Use correct argument for [AttachedPropertyBrowsableForType]",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}