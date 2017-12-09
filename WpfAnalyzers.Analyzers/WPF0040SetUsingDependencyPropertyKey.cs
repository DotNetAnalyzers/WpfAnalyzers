namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0040SetUsingDependencyPropertyKey
    {
        public const string DiagnosticId = "WPF0040";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "A readonly DependencyProperty must be set with DependencyPropertyKey.",
            messageFormat: "Set '{0}' using '{1}'",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A readonly DependencyProperty must be set with DependencyPropertyKey.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}