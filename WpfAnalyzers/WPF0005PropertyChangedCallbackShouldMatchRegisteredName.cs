namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0005PropertyChangedCallbackShouldMatchRegisteredName
    {
        internal const string DiagnosticId = "WPF0005";

        internal static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Name of PropertyChangedCallback should match registered name.",
            messageFormat: "Method '{0}' should be named '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Name of PropertyChangedCallback should match registered name.");
    }
}
