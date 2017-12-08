namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0105EventDeclarationRemoveHandlerInRemove
    {
        public const string DiagnosticId = "WPF0105";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Call RemoveHandler in remove.",
            messageFormat: "Call RemoveHandler in remove.",
            category: AnalyzerCategory.RoutedEvent,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Call RemoveHandler in remove.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}