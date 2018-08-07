namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0061ClrMethodShouldHaveDocs
    {
        public const string DiagnosticId = "WPF0061";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "CLR accessor for attached property should have documentation.",
            messageFormat: "CLR accessor for attached property should have documentation.",
            category: AnalyzerCategory.Documentation,
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            description: "CLR accessor for attached property should have documentation.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
