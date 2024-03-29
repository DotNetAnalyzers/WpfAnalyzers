﻿namespace WpfAnalyzers;

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
            context is { Node: InvocationExpressionSyntax invocation, ContainingSymbol.IsStatic: true })
        {
            if (DependencyProperty.Register.MatchAny(invocation, context.SemanticModel, context.CancellationToken) is { OwnerTypeArgument: { } ownerTypeArgument })
            {
                HandleArgument(context, ownerTypeArgument);
            }
            else if (invocation.TryGetArgumentAtIndex(0, out var argument))
            {
                if (DependencyProperty.AddOwner.Match(invocation, context.SemanticModel, context.CancellationToken) is { })
                {
                    HandleArgument(context, argument);
                }
                else if (DependencyProperty.OverrideMetadata.Match(invocation, context.SemanticModel, context.CancellationToken) is { } &&
                         invocation.Expression is MemberAccessExpressionSyntax { Expression: { } expression } &&
                         context.SemanticModel.TryGetSymbol(expression, context.CancellationToken, out var symbol) &&
                         BackingFieldOrProperty.Match(symbol) is { } backing &&
                         context.ContainingSymbol.ContainingType.IsAssignableTo(backing.ContainingType, context.SemanticModel.Compilation))
                {
                    HandleArgument(context, argument);
                }
            }
        }
    }

    private static void HandleArgument(SyntaxNodeAnalysisContext context, ArgumentSyntax argument)
    {
        if (argument.TryGetTypeofValue(context.SemanticModel, context.CancellationToken, out var ownerType) &&
            context.ContainingSymbol is { } &&
            !TypeSymbolComparer.Equal(ownerType, context.ContainingSymbol.ContainingType))
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0011ContainingTypeShouldBeRegisteredOwner, argument.GetLocation(), context.ContainingSymbol.ContainingType));
        }
    }
}
