namespace WpfAnalyzers
{
    using System.Collections.Immutable;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0011ContainingTypeShouldBeRegisteredOwner : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.WPF0011ContainingTypeShouldBeRegisteredOwner);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.InvocationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is InvocationExpressionSyntax invocation &&
                context.ContainingSymbol.IsStatic)
            {
                if (invocation.TryGetArgumentAtIndex(2, out var argument) &&
                    (DependencyProperty.Register.MatchAny(invocation, context.SemanticModel, context.CancellationToken) is { }))
                {
                    HandleArgument(context, argument);
                }
                else if (invocation.TryGetArgumentAtIndex(0, out argument))
                {
                    if (DependencyProperty.AddOwner.Match(invocation, context.SemanticModel, context.CancellationToken) is { })
                    {
                        HandleArgument(context, argument);
                    }
                    else if (DependencyProperty.TryGetOverrideMetadataCall(invocation, context.SemanticModel, context.CancellationToken, out _) &&
                             invocation.Expression is MemberAccessExpressionSyntax { Expression: { } expression } &&
                             context.SemanticModel.TryGetSymbol(expression, context.CancellationToken, out var symbol) &&
                             BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out var fieldOrProperty) &&
                             context.ContainingSymbol.ContainingType.IsAssignableTo(fieldOrProperty.ContainingType, context.SemanticModel.Compilation))
                    {
                        HandleArgument(context, argument);
                    }
                }
            }
        }

        private static void HandleArgument(SyntaxNodeAnalysisContext context, ArgumentSyntax argument)
        {
            if (argument.TryGetTypeofValue(context.SemanticModel, context.CancellationToken, out var ownerType) &&
                !TypeSymbolComparer.Equal(ownerType, context.ContainingSymbol.ContainingType))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0011ContainingTypeShouldBeRegisteredOwner, argument.GetLocation(), context.ContainingSymbol.ContainingType));
            }
        }
    }
}
