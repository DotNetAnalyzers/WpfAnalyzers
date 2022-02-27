namespace WpfAnalyzers;

using System.Collections.Immutable;
using System.Threading;

using Gu.Roslyn.AnalyzerExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class WPF0041SetMutableUsingSetCurrentValue : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Descriptors.WPF0041SetMutableUsingSetCurrentValue);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(x => HandleAssignment(x), SyntaxKind.SimpleAssignmentExpression);
        context.RegisterSyntaxNodeAction(x => HandleInvocation(x), SyntaxKind.InvocationExpression);
    }

    private static void HandleAssignment(SyntaxNodeAnalysisContext context)
    {
        if (!context.IsExcludedFromAnalysis() &&
            context.Node is AssignmentExpressionSyntax { Left: { } left } assignment &&
            !IsInObjectInitializer(assignment) &&
            !IsInConstructor(assignment) &&
            context.SemanticModel.TryGetSymbol(left, context.CancellationToken, out IPropertySymbol? property) &&
            !IsIgnored(property) &&
            ClrProperty.Match(property, context.SemanticModel, context.CancellationToken) is { BackingSet: { } backingSet } &&
            backingSet.FieldOrProperty.Type == KnownSymbols.DependencyProperty &&
            !IsAssignedCreatedInScope(left, context.SemanticModel, context.CancellationToken))
        {
            var propertyArgument = backingSet.CreateArgument(context.SemanticModel, context.Node.SpanStart).ToString();
            context.ReportDiagnostic(
                Diagnostic.Create(
                    Descriptors.WPF0041SetMutableUsingSetCurrentValue,
                    assignment.GetLocation(),
                    ImmutableDictionary<string, string?>.Empty.Add(nameof(BackingFieldOrProperty), propertyArgument),
                    propertyArgument,
                    assignment.Right));
        }

        static bool IsIgnored(IPropertySymbol property)
        {
            return property == KnownSymbols.FrameworkElement.DataContext ||
                   property == KnownSymbols.FrameworkElement.Style;
        }
    }

    private static void HandleInvocation(SyntaxNodeAnalysisContext context)
    {
        if (!context.IsExcludedFromAnalysis() &&
            context.Node is InvocationExpressionSyntax invocation &&
            !IsInConstructor(invocation) &&
            invocation is { Expression: { } invocationExpression, ArgumentList: { Arguments: { Count: 2 } arguments } } &&
            arguments.TryElementAt(0, out var propertyArg) &&
            propertyArg is { Expression: { } expression } &&
            DependencyObject.SetValue.Match(invocation, context.SemanticModel, context.CancellationToken) is { } &&
            context.SemanticModel.TryGetSymbol(expression, context.CancellationToken, out var symbol) &&
            BackingFieldOrProperty.Match(symbol) is { } backing &&
            backing.Type != KnownSymbols.DependencyPropertyKey &&
            !IsIgnored(backing.Symbol) &&
            !IsAssignedCreatedInScope(invocationExpression, context.SemanticModel, context.CancellationToken) &&
            context.ContainingProperty() is null)
        {
            if (context.ContainingSymbol is IMethodSymbol clrMethod &&
                SetAttached.Match(clrMethod, context.SemanticModel, context.CancellationToken) is { })
            {
                return;
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    Descriptors.WPF0041SetMutableUsingSetCurrentValue,
                    invocation.GetLocation(),
                    backing,
                    invocation.ArgumentList.Arguments[1]));
        }

        static bool IsIgnored(ISymbol field)
        {
            return field == KnownSymbols.FrameworkElement.DataContextProperty ||
                   field == KnownSymbols.FrameworkElement.StyleProperty;
        }
    }

    private static bool IsAssignedCreatedInScope(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return expression switch
        {
            MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax identifier }
                => semanticModel.TryGetSymbol(identifier, cancellationToken, out var symbol) &&
                   symbol.Kind == SymbolKind.Local,
            MemberAccessExpressionSyntax { Expression: { Parent: ObjectCreationExpressionSyntax _ } } => true,
            _ => false,
        };
    }

    private static bool IsInObjectInitializer(SyntaxNode node)
    {
        return node.Parent.IsKind(SyntaxKind.ObjectInitializerExpression);
    }

    private static bool IsInConstructor(SyntaxNode node)
    {
        return node.Parent is StatementSyntax statement &&
               statement.TryFirstAncestor<ConstructorDeclarationSyntax>(out _) &&
               !statement.TryFirstAncestor<AnonymousFunctionExpressionSyntax>(out _);
    }
}
