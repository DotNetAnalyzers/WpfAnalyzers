namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0104EventDeclarationAddHandlerInAdd
    {
        internal const string DiagnosticId = "WPF0104";

        internal static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Call AddHandler in add.",
            messageFormat: "Call AddHandler in add.",
            category: AnalyzerCategory.RoutedEvent,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Call AddHandler in add.");
    }
}
