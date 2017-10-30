namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0014SetValueMustUseRegisteredType : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0014";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "SetValue must use registered type.",
            messageFormat: "{0} must use registered type {1}",
            category: AnalyzerCategory.DependencyProperties,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: AnalyzerConstants.EnabledByDefault,
            description: "Use a type that matches registered type when setting the value of a DependencyProperty",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

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
                    var value = invocation.ArgumentList.Arguments[1];

                    if (value.Expression.IsSameType(KnownSymbol.Object, context))
                    {
                        return;
                    }

                    if (BackingFieldOrProperty.TryCreate(context.SemanticModel.GetSymbolSafe(invocation.ArgumentList.Arguments[0].Expression, context.CancellationToken), out var fieldOrProperty) &&
                        DependencyProperty.TryGetRegisteredType(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredType))
                    {
                        if (registeredType.Is(KnownSymbol.Freezable) &&
                            value.Expression.IsSameType(KnownSymbol.Freezable, context))
                        {
                            return;
                        }

                        if (!registeredType.IsRepresentationPreservingConversion(value.Expression, context.SemanticModel, context.CancellationToken))
                        {
                            var setCall = context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken);
                            context.ReportDiagnostic(Diagnostic.Create(Descriptor, value.GetLocation(), setCall.Name, registeredType));
                        }
                    }
                }
            }
        }
    }
}