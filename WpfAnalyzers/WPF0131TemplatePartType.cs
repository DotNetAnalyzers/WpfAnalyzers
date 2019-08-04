namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0131TemplatePartType
    {
        internal const string DiagnosticId = "WPF0131";

        internal static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Use correct [TemplatePart] type.",
            messageFormat: "Use correct [TemplatePart] type.",
            category: AnalyzerCategory.TemplatePart,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use correct [TemplatePart] type.");
    }
}
