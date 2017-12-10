namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0033UseAttachedPropertyBrowsableForTypeAttribute
    {
        public const string DiagnosticId = "WPF0033";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Add [AttachedPropertyBrowsableForType]",
            messageFormat: "Add [AttachedPropertyBrowsableForType(typeof({0})]",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Add [AttachedPropertyBrowsableForType]",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}