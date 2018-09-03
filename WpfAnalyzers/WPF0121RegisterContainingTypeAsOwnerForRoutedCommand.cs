namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0121RegisterContainingTypeAsOwnerForRoutedCommand
    {
        public const string DiagnosticId = "WPF0121";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Register containing type as owner for routed command.",
            messageFormat: "Register {0} as owner.",
            category: AnalyzerCategory.RoutedCommand,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Register containing type as owner for routed command.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
