namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0001BackingFieldShouldMatchRegisteredName
    {
        public const string DiagnosticId = "WPF0001";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Backing field for a DependencyProperty should match registered name.",
            messageFormat: "Field '{0}' that is backing field for the DependencyProperty registered as '{1}' must be named '{1}Property'",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A dependency property's backing field must be named with the name it is registered with suffixed by 'Property'",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
