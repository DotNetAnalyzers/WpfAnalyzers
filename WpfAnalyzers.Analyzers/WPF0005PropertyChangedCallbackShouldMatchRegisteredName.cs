namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0005PropertyChangedCallbackShouldMatchRegisteredName
    {
        public const string DiagnosticId = "WPF0005";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Name of PropertyChangedCallback should match registered name.",
            messageFormat: "Method '{0}' should be named '{1}'",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Name of PropertyChangedCallback should match registered name.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
