namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0100BackingFieldShouldMatchRegisteredName
    {
        internal const string DiagnosticId = "WPF0100";

        internal static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Backing field for a RoutedEvent should match registered name.",
            messageFormat: "Field '{0}' that is backing field for the RoutedEvent registered as '{1}' must be named '{1}Event'",
            category: AnalyzerCategory.RoutedEvent,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A routed event's backing field should be named with the name it is registered with suffixed by 'Event'");
    }
}
