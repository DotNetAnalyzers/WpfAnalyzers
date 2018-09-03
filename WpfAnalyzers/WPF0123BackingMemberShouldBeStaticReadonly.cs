namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0123BackingMemberShouldBeStaticReadonly
    {
        public const string DiagnosticId = "WPF0123";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Backing field for a RoutedCommand should be static and readonly.",
            messageFormat: "Backing member for a RoutedCommand and should be static and readonly.",
            category: AnalyzerCategory.RoutedCommand,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Backing field for a RoutedCommand should be static and readonly.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}
