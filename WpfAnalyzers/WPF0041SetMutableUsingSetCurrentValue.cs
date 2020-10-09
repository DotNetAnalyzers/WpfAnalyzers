namespace WpfAnalyzers
{
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
                property != KnownSymbols.FrameworkElement.DataContext &&
                ClrProperty.TrySingleBackingField(property, context.SemanticModel, context.CancellationToken, out var backing) &&
                !IsAssignedCreatedInScope(left, context.SemanticModel, context.CancellationToken))
            {
                var propertyArgument = backing.CreateArgument(context.SemanticModel, context.Node.SpanStart).ToString();
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.WPF0041SetMutableUsingSetCurrentValue,
                        assignment.GetLocation(),
                        properties: ImmutableDictionary<string, string>.Empty.Add(
                            nameof(BackingFieldOrProperty),
                            propertyArgument),
                        propertyArgument,
                        assignment.Right));
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
                DependencyObject.TryGetSetValueCall(invocation, context.SemanticModel, context.CancellationToken, out _) &&
                context.SemanticModel.TryGetSymbol(expression, context.CancellationToken, out var symbol) &&
                BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out var backing) &&
                backing.Type != KnownSymbols.DependencyPropertyKey &&
                backing.Symbol != KnownSymbols.FrameworkElement.DataContextProperty &&
                !IsAssignedCreatedInScope(invocationExpression, context.SemanticModel, context.CancellationToken))
            {
                if (context.ContainingProperty() is { } clrProperty &&
                    clrProperty.IsDependencyPropertyAccessor(context.SemanticModel, context.CancellationToken))
                {
                    return;
                }

                if (context.ContainingSymbol is IMethodSymbol clrMethod &&
                    ClrMethod.IsAttachedSet(clrMethod, context.SemanticModel, context.CancellationToken, out backing))
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
        }

        private static bool IsAssignedCreatedInScope(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return expression switch
            {
                MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax identifier } => semanticModel.TryGetSymbol(identifier, cancellationToken, out var symbol) &&
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
}
