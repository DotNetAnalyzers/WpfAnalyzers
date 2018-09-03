namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0082ConstructorArgument
    {
        public const string DiagnosticId = "WPF0082";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "[ConstructorArgument] must match.",
            messageFormat: "[ConstructorArgument] must match. Expected: {0}",
            category: AnalyzerCategory.MarkupExtension,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "[ConstructorArgument] must match the name of the constructor parameter.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
