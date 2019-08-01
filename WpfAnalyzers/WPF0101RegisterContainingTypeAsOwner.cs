namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0101RegisterContainingTypeAsOwner
    {
        internal const string DiagnosticId = "WPF0101";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Containing type should be used as registered owner.",
            messageFormat: "Register containing type: '{0}' as owner.",
            category: AnalyzerCategory.RoutedEvent,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When registering a RoutedEvent register containing type as owner type.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}