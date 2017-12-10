namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0019CastSenderToCorrectType
    {
        public const string DiagnosticId = "WPF0019";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Cast sender to correct type.",
            messageFormat: "Sender is of type {0}.",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Cast sender to correct type.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}