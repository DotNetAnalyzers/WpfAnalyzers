namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0030BackingFieldShouldBeStaticReadonly
    {
        internal const string DiagnosticId = "WPF0030";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Backing field for a DependencyProperty should be static and readonly.",
            messageFormat: "Field '{0}' is backing field for a DependencyProperty and should be static and readonly.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Backing field for a DependencyProperty should be static and readonly.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
