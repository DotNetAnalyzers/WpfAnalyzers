namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0034AttachedPropertyBrowsableForTypeAttributeArgument
    {
        internal const string DiagnosticId = "WPF0034";

        internal static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Use correct argument for [AttachedPropertyBrowsableForType]",
            messageFormat: "Use [AttachedPropertyBrowsableForType(typeof({0})]",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Use correct argument for [AttachedPropertyBrowsableForType]");
    }
}
