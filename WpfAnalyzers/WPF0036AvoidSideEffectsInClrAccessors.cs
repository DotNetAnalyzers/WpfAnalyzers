namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0036AvoidSideEffectsInClrAccessors
    {
        internal const string DiagnosticId = "WPF0036";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Avoid side effects in CLR accessors.",
            messageFormat: "Avoid side effects in CLR accessors.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Avoid side effects in CLR accessors.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
