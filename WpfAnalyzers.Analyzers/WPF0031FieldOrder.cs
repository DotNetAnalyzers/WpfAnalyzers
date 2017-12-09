namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0031FieldOrder
    {
        public const string DiagnosticId = "WPF0031";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "DependencyPropertyKey field must come before DependencyProperty field.",
            messageFormat: "Field '{0}' must come before '{1}'",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "DependencyPropertyKey field must come before DependencyProperty field.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}