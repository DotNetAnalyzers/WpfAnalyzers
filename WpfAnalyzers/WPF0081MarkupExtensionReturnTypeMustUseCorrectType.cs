namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0081MarkupExtensionReturnTypeMustUseCorrectType
    {
        internal const string DiagnosticId = "WPF0081";

        internal static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "MarkupExtensionReturnType must use correct return type.",
            messageFormat: "MarkupExtensionReturnType must use correct return type. Expected: {0}",
            category: AnalyzerCategory.MarkupExtension,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "MarkupExtensionReturnType must use correct return type.");
    }
}
