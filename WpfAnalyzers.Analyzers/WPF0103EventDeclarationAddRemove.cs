namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0103EventDeclarationAddRemove
    {
        public const string DiagnosticId = "WPF0103";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use same event in add and remove.",
            messageFormat: "Add uses: '{0}', remove uses: '{1}'.",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use same event in add and remove.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}