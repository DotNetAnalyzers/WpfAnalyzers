namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0073ConverterDoesNotHaveAttributeUnknownTypes
    {
        public const string DiagnosticId = "WPF0073";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Add ValueConversion attribute (unknown types).",
            messageFormat: "Add ValueConversion attribute (unknown types).",
            category: AnalyzerCategory.IValueConverter,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Add ValueConversion attribute (unknown types).",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}