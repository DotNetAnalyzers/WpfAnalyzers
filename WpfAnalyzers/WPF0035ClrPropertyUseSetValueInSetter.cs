namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0035ClrPropertyUseSetValueInSetter
    {
        internal const string DiagnosticId = "WPF0035";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use SetValue in setter.",
            messageFormat: "Use SetValue in setter.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use SetValue in setter.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
