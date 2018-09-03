namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0130UseTemplatePartAttribute
    {
        public const string DiagnosticId = "WPF0130";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Add [TemplatePart] to the type.",
            messageFormat: "Add {0} to the type.",
            category: AnalyzerCategory.TemplatePart,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Add [TemplatePart] to the type.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
