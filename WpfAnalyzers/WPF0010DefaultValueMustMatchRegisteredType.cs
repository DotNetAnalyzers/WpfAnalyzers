namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0010DefaultValueMustMatchRegisteredType
    {
        internal const string DiagnosticId = "WPF0010";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Default value type must match registered type.",
            messageFormat: "Default value for '{0}' must be of type {1}",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A DependencyProperty is registered with a type and a default value. The type of the default value must be the same as the registered type.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
