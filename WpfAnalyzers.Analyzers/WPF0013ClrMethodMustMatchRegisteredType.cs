namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal static class WPF0013ClrMethodMustMatchRegisteredType
    {
        public const string DiagnosticId = "WPF0013";

        internal static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "CLR accessor for attached property must match registered type.",
            messageFormat: "{0} must match registered type {1}",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "CLR accessor for attached property must match registered type.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));
    }
}