namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0019CastSenderToCorrectType
    {
        internal const string DiagnosticId = "WPF0019";

        internal static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Cast sender to correct type.",
            messageFormat: "Sender is of type {0}.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Cast sender to correct type.");
    }
}
