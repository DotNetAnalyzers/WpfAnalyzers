namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0131TemplatePartType
    {
        public const string DiagnosticId = "WPF0131";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use correct [TemplatePart] type.",
            messageFormat: "Use correct [TemplatePart] type.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use correct [TemplatePart] type.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
