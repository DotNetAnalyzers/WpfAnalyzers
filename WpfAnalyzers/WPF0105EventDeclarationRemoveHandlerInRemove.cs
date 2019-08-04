namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0105EventDeclarationRemoveHandlerInRemove
    {
        internal static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: "WPF0105",
            title: "Call RemoveHandler in remove.",
            messageFormat: "Call RemoveHandler in remove.",
            category: AnalyzerCategory.RoutedEvent,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Call RemoveHandler in remove.");
    }
}
