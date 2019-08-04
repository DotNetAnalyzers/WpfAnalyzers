namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0012ClrPropertyShouldMatchRegisteredType
    {
        internal const string DiagnosticId = "WPF0012";

        internal static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "CLR property type should match registered type.",
            messageFormat: "Property '{0}' must be of type {1}",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "CLR property type should match registered type.");
    }
}
