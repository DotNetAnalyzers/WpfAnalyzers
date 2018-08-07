namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0061DocumentClrMethod
    {
        public const string DiagnosticId = "WPF0061";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Accessor method for attached property must have standard documentation text.",
            messageFormat: "Accessor method for attached property must have standard documentation text.",
            category: AnalyzerCategory.Documentation,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Accessor method for attached property must have standard documentation text.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
