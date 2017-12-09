namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0070ConverterDoesNotHaveDefaultField
    {
        public const string DiagnosticId = "WPF0070";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Add default field to converter.",
            messageFormat: "Add default field to converter.",
            category: AnalyzerCategory.IValueConverter,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Add default field to converter.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}