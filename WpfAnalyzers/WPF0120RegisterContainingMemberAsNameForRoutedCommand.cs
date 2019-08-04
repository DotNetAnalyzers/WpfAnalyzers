namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class WPF0120RegisterContainingMemberAsNameForRoutedCommand
    {
        internal const string DiagnosticId = "WPF0120";

        internal static readonly DiagnosticDescriptor Descriptor = Descriptors.Create(
            id: DiagnosticId,
            title: "Register containing member name as name for routed command.",
            messageFormat: "Register {0} as name.",
            category: AnalyzerCategory.RoutedCommand,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Register containing member name as name for routed command.");
    }
}
