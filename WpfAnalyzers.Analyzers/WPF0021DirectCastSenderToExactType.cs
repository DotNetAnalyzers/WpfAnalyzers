namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0021DirectCastSenderToExactType
    {
        public const string DiagnosticId = "WPF0021";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Cast sender to containing type.",
            messageFormat: "Sender is of type {0}.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Cast sender to correct type.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}