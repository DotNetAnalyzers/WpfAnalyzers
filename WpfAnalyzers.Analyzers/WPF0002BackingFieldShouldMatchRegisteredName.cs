namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0002BackingFieldShouldMatchRegisteredName
    {
        public const string DiagnosticId = "WPF0002";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Backing field for a DependencyPropertyKey should match registered name.",
            messageFormat: "Field '{0}' that is backing field for the DependencyPropertyKey registered as '{1}' must be named '{1}PropertyKey'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A DependencyPropertyKey's backing field must be named with the name it is registered with suffixed by 'PropertyKey'",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}