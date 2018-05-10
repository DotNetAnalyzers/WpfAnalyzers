namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0060DocumentDependencyPropertyBackingField
    {
        public const string DiagnosticId = "WPF0060";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Backing field for a DependencyProperty is missing docs.",
            messageFormat: "Backing field for a DependencyProperty is missing docs.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            description: "Backing field for a DependencyProperty is missing docs.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}