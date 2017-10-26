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
                if (DependencyObject.TryGetSetValueCall(invocation, context.SemanticModel, context.CancellationToken, out _) ||
                    DependencyObject.TryGetSetCurrentValueCall(invocation, context.SemanticModel, context.CancellationToken, out _))
                {
                    var propertyArg = invocation.ArgumentList.Arguments[0];
                    if (BackingFieldOrProperty.TryCreate(context.SemanticModel.GetSymbolSafe(propertyArg.Expression, context.CancellationToken), out var fieldOrProperty) &&
                        fieldOrProperty.Type == KnownSymbol.DependencyProperty)
                    {
                        if (DependencyProperty.TryGetDependencyPropertyKeyField(
                            fieldOrProperty,
                            context.SemanticModel,
                            context.CancellationToken,
                            out var keyField))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptor,
                                    propertyArg.GetLocation(),
                                    propertyArg,
                                    keyField.CreateArgument(context.SemanticModel, propertyArg.SpanStart)));
                        }
                    }
                }
            }
        }
    }
}