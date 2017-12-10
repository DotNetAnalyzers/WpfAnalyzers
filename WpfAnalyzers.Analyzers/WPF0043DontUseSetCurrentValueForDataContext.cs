namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0043DontUseSetCurrentValueForDataContext
    {
        public const string DiagnosticId = "WPF0043";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Don't set DataContext using SetCurrentValue.",
            messageFormat: "Use SetValue({0}, {1})",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Set DataContext using SetValue.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}