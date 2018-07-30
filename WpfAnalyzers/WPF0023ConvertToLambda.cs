namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0023ConvertToLambda
    {
        public const string DiagnosticId = "WPF0023";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "The callback is trivial, convert to lambda.",
            messageFormat: "Convert to lambda.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "The callback is trivial, convert to lambda for better locality.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
