namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0122RegisterRoutedCommand
    {
        public const string DiagnosticId = "WPF0122";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Register name and owning type for routed command.",
            messageFormat: "Register name and owning type for routed command.",
            category: AnalyzerCategory.RoutedEvent,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Register containing type as owner for routed command.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
