namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0141UseContainingMemberComponentResourceKey
    {
        public const string DiagnosticId = "WPF0141";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use containing member as key when creating a ComponentResourceKey.",
            messageFormat: "Use containing member: {0}.",
            category: AnalyzerCategory.ComponentResourceKey,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use containing member as key when creating a ComponentResourceKey.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
