namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0103EventDeclarationAddRemove
    {
        internal const string DiagnosticId = "WPF0103";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use same event in add and remove.",
            messageFormat: "Add uses: '{0}', remove uses: '{1}'.",
            category: AnalyzerCategory.RoutedEvent,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use same event in add and remove.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}