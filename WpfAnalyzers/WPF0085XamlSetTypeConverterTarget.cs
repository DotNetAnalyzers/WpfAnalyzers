namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal class WPF0085XamlSetTypeConverterTarget
    {
        internal const string DiagnosticId = "WPF0085";

        internal static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Target of [XamlSetTypeConverter] should exist and have correct signature.",
            messageFormat: "Expected a method with signature void ReceiveTypeConverter(object, XamlSetTypeConverterEventArgs).",
            category: AnalyzerCategory.MarkupExtension,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Target of [XamlSetTypeConverter] should exist and have correct signature.");
    }
}
