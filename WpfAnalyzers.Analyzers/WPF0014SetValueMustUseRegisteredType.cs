namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0014SetValueMustUseRegisteredType
    {
        public const string DiagnosticId = "WPF0014";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "SetValue must use registered type.",
            messageFormat: "{0} must use registered type {1}",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Use a type that matches registered type when setting the value of a DependencyProperty",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}