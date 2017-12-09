namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0072ValueConversionMustUseCorrectTypes
    {
        public const string DiagnosticId = "WPF0072";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "ValueConversion must use correct types.",
            messageFormat: "ValueConversion must use correct types. Expected: {0}",
            category: AnalyzerCategory.IValueConverter,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "ValueConversion must use correct types.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}