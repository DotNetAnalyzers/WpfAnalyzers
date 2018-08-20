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
        public const string DiagnosticId = "WPF0041";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Set mutable dependency properties using SetCurrentValue.",
            messageFormat: "Use SetCurrentValue({0}, {1})",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer setting mutable dependency properties using SetCurrentValue.",
            helpLinkUri: HelpLink.ForId(DiagnosticId));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleAssignment, SyntaxKind.SimpleAssignmentExpression);
            context.RegisterSyntaxNodeAction(HandleInvocation, SyntaxKind.InvocationExpression);
        }

        private static void HandleAssignment(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is AssignmentExpressionSyntax assignment)
            {
                if (IsInObjectInitializer(assignment) ||
                    IsInConstructor(assignment))
                {
                    return;
                }

                var property = context.SemanticModel.GetSymbolSafe(assignment.Left, context.CancellationToken) as IPropertySymbol;
                if (property == KnownSymbol.FrameworkElement.DataContext)
                {
                    return;
                }

                if (ClrProperty.TrySingleBackingField(property, context.SemanticModel, context.CancellationToken, out var fieldOrProperty))
                {
                    if (IsCalleePotentiallyCreatedInScope(assignment.Left as MemberAccessExpressionSyntax, context.SemanticModel, context.CancellationToken))
                    {
                        return;
                    }

                    var propertyArg = fieldOrProperty.CreateArgument(context.SemanticModel, context.Node.SpanStart);
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, assignment.GetLocation(), propertyArg, assignment.Right));
                }
            }
        }

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is InvocationExpressionSyntax invocation &&
                !IsInObjectInitializer(invocation) &&
                !IsInConstructor(invocation) &&
                DependencyObject.TryGetSetValueCall(invocation, context.SemanticModel, context.CancellationToken, out _))
            {
                var propertyArg = invocation.ArgumentList.Arguments[0];
                if (!BackingFieldOrProperty.TryCreateForDependencyProperty(context.SemanticModel.GetSymbolSafe(propertyArg.Expression, context.CancellationToken), out var propertyMember) ||
                    propertyMember.Type == KnownSymbol.DependencyPropertyKey)
                {
                    return;
                }

                if (propertyMember.Symbol is IFieldSymbol field &&
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
                if (ClrMethod.IsAttachedSet(clrMethod, context.SemanticModel, context.CancellationToken, out propertyMember))
                {
                    return;
                }

                if (IsCalleePotentiallyCreatedInScope(invocation.Expression as MemberAccessExpressionSyntax, context.SemanticModel, context.CancellationToken))
                {
                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.GetLocation(), propertyMember, invocation.ArgumentList.Arguments[1]));
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
