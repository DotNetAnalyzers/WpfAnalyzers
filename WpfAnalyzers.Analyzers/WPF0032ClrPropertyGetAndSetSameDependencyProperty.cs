namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0032ClrPropertyGetAndSetSameDependencyProperty
    {
        public const string DiagnosticId = "WPF0032";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use same dependency property in get and set.",
            messageFormat: "Property '{0}' must access same dependency property in getter and setter",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Use same dependency property in get and set.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}