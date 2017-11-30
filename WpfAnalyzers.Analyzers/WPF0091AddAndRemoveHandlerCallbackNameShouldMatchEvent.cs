namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0091AddAndRemoveHandlerCallbackNameShouldMatchEvent
    {
        public const string DiagnosticId = "WPF0091";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Name the invoked method OnEventName.",
            messageFormat: "Rename to {0} to match the event.",
            category: AnalyzerCategory.RoutedEvent,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Name the invoked method OnEventName.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}