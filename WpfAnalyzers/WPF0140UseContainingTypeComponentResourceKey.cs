namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0140UseContainingTypeComponentResourceKey
    {
        public const string DiagnosticId = "WPF0140";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use containing type when creating a ComponentResourceKey.",
            messageFormat: "Use containing type: {0}.",
            category: AnalyzerCategory.ComponentResourceKey,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use containing type when creating a ComponentResourceKey.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
