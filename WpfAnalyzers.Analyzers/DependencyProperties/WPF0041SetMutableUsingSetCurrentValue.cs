namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0041SetMutableUsingSetCurrentValue : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0041";
        private const string Title = "Set mutable dependency properties using SetCurrentValue.";
        private const string MessageFormat = "Use SetCurrentValue({0}, {1})";
        private const string Description = "Prefer setting mutable dependency properties using SetCurrentValue.";
        private static readonly string HelpLink = WpfAnalyzers.HelpLink.ForId(DiagnosticId);

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
                                                                      DiagnosticId,
                                                                      Title,
                                                                      MessageFormat,
                                                                      AnalyzerCategory.DependencyProperties,
                                                                      DiagnosticSeverity.Warning,
                                                                      AnalyzerConstants.EnabledByDefault,
                                                                      Description,
                                                                      HelpLink);

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
            if (IsInObjectInitializer(context.Node) ||
                IsInConstructor(context.Node))
            {
                return;
            }

            var assignment = context.Node as AssignmentExpressionSyntax;
            if (assignment == null ||
                assignment.IsMissing ||
                context.SemanticModel == null)
            {
                return;
            }

            var property = context.SemanticModel.GetSymbolInfo(assignment.Left, context.CancellationToken).Symbol as IPropertySymbol;

            IFieldSymbol field;
            if (ClrProperty.TryGetSingleBackingField(property, context.SemanticModel, context.CancellationToken, out field))
            {
                if (IsCalleePotentiallyCreatedInScope(assignment.Left as MemberAccessExpressionSyntax, context.SemanticModel, context.CancellationToken))
                {
                    return;
                }

                var propertyArg = DependencyProperty.CreateArgument(field, context.SemanticModel, context.Node.SpanStart);
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, assignment.GetLocation(), propertyArg, assignment.Right));
            }
        }

        private static void HandleInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation == null || context.SemanticModel == null)
            {
                return;
            }

            if (IsInObjectInitializer(context.Node) ||
                IsInConstructor(context.Node))
            {
                return;
            }

            ArgumentSyntax property;
            ArgumentSyntax value;
            IFieldSymbol setField;
            if (!DependencyObject.TryGetSetValueArguments(invocation, context.SemanticModel, context.CancellationToken, out property, out setField, out value))
            {
                return;
            }

            if (setField == null ||
                setField.Type != QualifiedType.DependencyProperty)
            {
                return;
            }

            var clrProperty = context.ContainingProperty();
            if (ClrProperty.IsDependencyPropertyAccessor(clrProperty, context.SemanticModel, context.CancellationToken))
            {
                return;
            }

            var clrMethod = context.ContainingSymbol as IMethodSymbol;
            if (ClrMethod.IsAttachedSetMethod(clrMethod, context.SemanticModel, context.CancellationToken, out setField))
            {
                return;
            }

            if (IsCalleePotentiallyCreatedInScope(invocation.Expression as MemberAccessExpressionSyntax, context.SemanticModel, context.CancellationToken))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.GetLocation(), property, value));
        }

        private static bool IsCalleePotentiallyCreatedInScope(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (memberAccess == null ||
                !memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression) ||
                memberAccess.Expression.IsKind(SyntaxKind.ThisExpression))
            {
                return false;
            }

            var callee = memberAccess.Expression as IdentifierNameSyntax;
            if (callee == null)
            {
                return false;
            }

            var symbol = semanticModel.GetSymbolInfo(callee, cancellationToken).Symbol;
            if (symbol.Kind != SymbolKind.Local)
            {
                return false;
            }

            SyntaxReference reference;
            if (!symbol.DeclaringSyntaxReferences.TryGetSingle(out reference))
            {
                return false;
            }

            var declarator = reference.GetSyntax(cancellationToken) as VariableDeclaratorSyntax;
            var objectCreation = declarator?.Initializer?.Value as ObjectCreationExpressionSyntax;
            if (objectCreation == null)
            {
                return false;
            }

            return true;
        }

        private static bool IsInObjectInitializer(SyntaxNode node)
        {
            return node.Parent.IsKind(SyntaxKind.ObjectInitializerExpression);
        }

        private static bool IsInConstructor(SyntaxNode node)
        {
            var statement = node.Parent as StatementSyntax;
            var blockSyntax = statement?.Parent as BlockSyntax;
            if (blockSyntax == null)
            {
                return false;
            }

            return blockSyntax.Parent.IsKind(SyntaxKind.ConstructorDeclaration);
        }
    }
}