namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0001BackingFieldShouldMatchRegisteredName
    {
        public const string DiagnosticId = "WPF0001";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Backing field for a DependencyProperty should match registered name.",
            messageFormat: "Field '{0}' that is backing field for the DependencyProperty registered as '{1}' should be named '{1}Property'.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A dependency property's backing field should be named with the name it is registered with suffixed by 'Property'.\r\n" +
                         "This is the convention in the framework.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
