namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0018DefaultStyleKeyPropertyOverrideMetadataArgument
    {
        internal const string DiagnosticId = "WPF0018";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use containing type.",
            messageFormat: "Expected new FrameworkPropertyMetadata(typeof({0}))",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Call DefaultStyleKeyProperty.OverrideMetadata with containing type as argument.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
