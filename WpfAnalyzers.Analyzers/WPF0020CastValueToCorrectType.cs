namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0020CastValueToCorrectType
    {
        public const string DiagnosticId = "WPF0020";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Cast value to correct type.",
            messageFormat: "Value is of type {0}.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Cast value to correct type.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}