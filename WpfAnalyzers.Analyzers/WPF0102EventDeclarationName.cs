namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0102EventDeclarationName
    {
        public const string DiagnosticId = "WPF0102";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Name of the event should match registered name.",
            messageFormat: "Rename to: '{0}'.",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Name of the event should match registered name.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}