namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0104EventDeclarationAddHandlerInAdd
    {
        public const string DiagnosticId = "WPF0104";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Call AddHandler in add.",
            messageFormat: "Call AddHandler in add.",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Call AddHandler in add.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}