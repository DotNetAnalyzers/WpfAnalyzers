namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0062DocumentPropertyChangedCallback
    {
        internal const string DiagnosticId = "WPF0062";

        internal static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Property changed callback must have standard documentation text.",
            messageFormat: "Property changed callback must have standard documentation text.",
            category: AnalyzerCategory.Documentation,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Property changed callback must have standard documentation text.");
    }
}
