namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0074DefaultMemberOfWrongType
    {
        public const string DiagnosticId = "WPF0074";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use containing type.",
            messageFormat: "Use containing type.",
            category: AnalyzerCategory.IValueConverter,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use containing type.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
