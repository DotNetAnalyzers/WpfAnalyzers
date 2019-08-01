namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0107BackingMemberShouldBeStaticReadonly
    {
        internal const string DiagnosticId = "WPF0107";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Backing member for a RoutedEvent should be static and readonly.",
            messageFormat: "Backing member for a RoutedEvent and should be static and readonly.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Backing member for a RoutedEvent should be static and readonly.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}