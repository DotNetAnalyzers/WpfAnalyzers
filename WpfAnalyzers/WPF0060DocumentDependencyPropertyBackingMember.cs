namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0060DocumentDependencyPropertyBackingMember
    {
        internal const string DiagnosticId = "WPF0060";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Backing member for DependencyProperty must have standard documentation text.",
            messageFormat: "Backing member for DependencyProperty must have standard documentation text.",
            category: AnalyzerCategory.Documentation,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Backing member for DependencyProperty must have standard documentation text.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
