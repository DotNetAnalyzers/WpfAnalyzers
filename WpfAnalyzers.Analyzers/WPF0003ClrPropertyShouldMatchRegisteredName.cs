namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal class WPF0003ClrPropertyShouldMatchRegisteredName
    {
        public const string DiagnosticId = "WPF0003";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "CLR property for a DependencyProperty should match registered name.",
            messageFormat: "Property '{0}' must be named '{1}'",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A CLR property accessor for a DependencyProperty must have the same name as the DependencyProperty is registered with.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}