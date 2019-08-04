namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0021DirectCastSenderToExactType
    {
        internal const string DiagnosticId = "WPF0021";

        internal static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Cast sender to containing type.",
            messageFormat: "Sender is of type {0}.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Cast sender to correct type.");
    }
}
