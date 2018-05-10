namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0016DefaultValueIsSharedReferenceType
    {
        public const string DiagnosticId = "WPF0016";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Default value is shared reference type.",
            messageFormat: "Default value for '{0}' is a reference type that will be shared among all instances.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When registering a new instance of a reference type as default value the value is shared for all instances of the control.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}