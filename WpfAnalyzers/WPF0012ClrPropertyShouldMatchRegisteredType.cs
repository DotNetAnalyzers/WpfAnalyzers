namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0012ClrPropertyShouldMatchRegisteredType
    {
        public const string DiagnosticId = "WPF0012";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "CLR property type should match registered type.",
            messageFormat: "Property '{0}' must be of type {1}",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "CLR property type should match registered type.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}