namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0013ClrMethodMustMatchRegisteredType
    {
        public const string DiagnosticId = "WPF0013";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "CLR accessor for attached property must match registered type.",
            messageFormat: "{0} must match registered type {1}",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "CLR accessor for attached property must match registered type.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}