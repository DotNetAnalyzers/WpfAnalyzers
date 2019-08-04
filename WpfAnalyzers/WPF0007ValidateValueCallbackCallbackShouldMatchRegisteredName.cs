namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName
    {
        internal const string DiagnosticId = "WPF0007";

        internal static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Name of ValidateValueCallback should match registered name.",
            messageFormat: "Method '{0}' should be named '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Name of ValidateValueCallback should match registered name.");
    }
}
