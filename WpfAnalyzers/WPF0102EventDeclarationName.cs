namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0102EventDeclarationName
    {
        internal const string DiagnosticId = "WPF0102";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Name of the event should match registered name.",
            messageFormat: "Rename to: '{0}'.",
            category: AnalyzerCategory.RoutedEvent,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Name of the event should match registered name.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}