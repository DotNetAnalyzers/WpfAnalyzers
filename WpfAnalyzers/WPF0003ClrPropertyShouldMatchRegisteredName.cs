namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0003ClrPropertyShouldMatchRegisteredName
    {
        internal const string DiagnosticId = "WPF0003";

        internal static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "CLR property for a DependencyProperty should match registered name.",
            messageFormat: "Property '{0}' must be named '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A CLR property accessor for a DependencyProperty must have the same name as the DependencyProperty is registered with.");
    }
}
