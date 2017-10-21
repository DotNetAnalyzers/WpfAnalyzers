namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0040SetUsingDependencyPropertyKey : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0040";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "A readonly DependencyProperty must be set with DependencyPropertyKey.",
            messageFormat: "Set '{0}' using '{1}'",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "A readonly DependencyProperty must be set with DependencyPropertyKey.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleInvocation, SyntaxKind.InvocationExpression);
        }

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is InvocationExpressionSyntax invocation)
            {
                if (!DependencyObject.TryGetSetValueArguments(
                        invocation,
                        context.SemanticModel,
                        context.CancellationToken,
                        out var property,
                        out var setField,
                        out _) &&
                    !DependencyObject.TryGetSetCurrentValueArguments(
                        invocation,
                        context.SemanticModel,
                        context.CancellationToken,
                        out property,
                        out setField,
                        out _))
                {
                    return;
                }

                if (setField == null ||
                    setField.Type == KnownSymbol.DependencyPropertyKey)
                {
                    return;
                }

                if (DependencyProperty.TryGetDependencyPropertyKeyField(
                    setField,
                    context.SemanticModel,
                    context.CancellationToken,
                    out var keyField))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptor,
                            property.GetLocation(),
                            property,
                            DependencyProperty.CreateArgument(keyField, context.SemanticModel, property.SpanStart)));
                }
            }
        }
    }
}