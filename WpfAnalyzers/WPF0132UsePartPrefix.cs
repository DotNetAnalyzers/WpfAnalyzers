namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0132UsePartPrefix
    {
        internal const string DiagnosticId = "WPF0132";

        internal static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Use PART prefix.",
            messageFormat: "Use PART prefix.",
            category: AnalyzerCategory.TemplatePart,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use PART prefix.");
    }
}
