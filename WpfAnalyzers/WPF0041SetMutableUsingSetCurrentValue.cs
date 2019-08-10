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
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptors.WPF0041SetMutableUsingSetCurrentValue);

        /// <inheritdoc/>
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
                context.Node is AssignmentExpressionSyntax assignment &&
                !IsInObjectInitializer(assignment) &&
                !IsInConstructor(assignment) &&
                context.SemanticModel.TryGetSymbol(assignment.Left, context.CancellationToken, out IPropertySymbol property) &&
                property != KnownSymbol.FrameworkElement.DataContext &&
                ClrProperty.TrySingleBackingField(property, context.SemanticModel, context.CancellationToken, out var fieldOrProperty) &&
                !IsCalleePotentiallyCreatedInScope(assignment.Left as MemberAccessExpressionSyntax, context.SemanticModel, context.CancellationToken))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.WPF0041SetMutableUsingSetCurrentValue,
                        assignment.GetLocation(),
                        properties: ImmutableDictionary<string, string>.Empty.Add(
                            nameof(BackingFieldOrProperty),
                            $"{fieldOrProperty.Symbol.ContainingType.ToMinimalDisplayString(context.SemanticModel, context.Node.SpanStart)}.{fieldOrProperty.Name}"),
                        fieldOrProperty.CreateArgument(context.SemanticModel, context.Node.SpanStart),
                        assignment.Right));
            }
        }

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is InvocationExpressionSyntax invocation &&
                !IsInConstructor(invocation) &&
                invocation.ArgumentList is ArgumentListSyntax argumentList &&
                argumentList.Arguments.Count == 2 &&
                argumentList.Arguments.TryElementAt(0, out var propertyArg) &&
                DependencyObject.TryGetSetValueCall(invocation, context.SemanticModel, context.CancellationToken, out _) &&
                BackingFieldOrProperty.TryCreateForDependencyProperty(context.SemanticModel.GetSymbolSafe(propertyArg.Expression, context.CancellationToken), out var backingFieldOrProperty) &&
                !IsCalleePotentiallyCreatedInScope(invocation.Expression as MemberAccessExpressionSyntax, context.SemanticModel, context.CancellationToken))
            {
                if (backingFieldOrProperty.Type == KnownSymbol.DependencyPropertyKey)
                {
                    return;
                }

                if (backingFieldOrProperty.Symbol is IFieldSymbol field &&
                    field == KnownSymbol.FrameworkElement.DataContextProperty)
                {
                    return;
                }

                var clrProperty = context.ContainingProperty();
                if (clrProperty.IsDependencyPropertyAccessor(context.SemanticModel, context.CancellationToken))
                {
                    return;
                }

                var clrMethod = context.ContainingSymbol as IMethodSymbol;
                if (ClrMethod.IsAttachedSet(clrMethod, context.SemanticModel, context.CancellationToken, out backingFieldOrProperty))
                {
                    return;
                }

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.WPF0041SetMutableUsingSetCurrentValue,
                        invocation.GetLocation(),
                        backingFieldOrProperty,
                        invocation.ArgumentList.Arguments[1]));
            }
        }

        private static bool IsCalleePotentiallyCreatedInScope(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (memberAccess == null ||
                !memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression) ||
                memberAccess.Expression.IsKind(SyntaxKind.ThisExpression))
            {
                return false;
            }

            if (memberAccess.Expression is IdentifierNameSyntax callee)
            {
                var symbol = semanticModel.GetSymbolSafe(callee, cancellationToken);
                if (symbol.Kind != SymbolKind.Local)
                {
                    return false;
                }

                if (!symbol.DeclaringSyntaxReferences.TrySingle(out var reference))
                {
                    return false;
                }

                return reference.GetSyntax(cancellationToken) is VariableDeclaratorSyntax variableDeclarator &&
                       variableDeclarator.Initializer is EqualsValueClauseSyntax initializer &&
                       initializer.Value is ObjectCreationExpressionSyntax;
            }

            return false;
        }

        private static bool IsInObjectInitializer(SyntaxNode node)
        {
            return node.Parent.IsKind(SyntaxKind.ObjectInitializerExpression);
        }

        private static bool IsInConstructor(SyntaxNode node)
        {
            return node.Parent is StatementSyntax statement &&
                   statement.Parent is BlockSyntax blockSyntax &&
                   blockSyntax.Parent.IsKind(SyntaxKind.ConstructorDeclaration);
        }
    }
}
