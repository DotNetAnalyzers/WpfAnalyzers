namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0042AvoidSideEffectsInClrAccessors
    {
        public const string DiagnosticId = "WPF0042";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Avoid side effects in CLR accessors.",
            messageFormat: "Avoid side effects in CLR accessors.",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Avoid side effects in CLR accessors.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}