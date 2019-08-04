namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0071ConverterDoesNotHaveAttribute
    {
        internal const string DiagnosticId = "WPF0071";

        internal static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Add ValueConversion attribute.",
            messageFormat: "Add ValueConversion attribute.",
            category: AnalyzerCategory.IValueConverter,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Add ValueConversion attribute.");
    }
}
