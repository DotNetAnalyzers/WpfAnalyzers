namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseSetCurrentValueCodeFixProvider))]
    [Shared]
    internal class UseSetCurrentValueCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(WPF0041SetMutableUsingSetCurrentValue.DiagnosticId);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "SetCurrentValue()",
                        _ => ApplyFixAsync(context, diagnostic),
                        nameof(MakeFieldStaticReadonlyCodeFixProvider)),
                    diagnostic);
            }

            return FinishedTasks.Task;
        }

        private static async Task<Document> ApplyFixAsync(CodeFixContext context, Diagnostic diagnostic)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            var assignment = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                       .FirstAncestorOrSelf<AssignmentExpressionSyntax>();
            if (assignment != null && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                return await ApplyFixAsync(context, syntaxRoot, assignment).ConfigureAwait(false);
            }

            var invocation = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                       .FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocation != null)
            {
                return await ApplyFixAsync(context, syntaxRoot, invocation).ConfigureAwait(false);
            }

            return context.Document;
        }

        private static async Task<Document> ApplyFixAsync(CodeFixContext context, SyntaxNode syntaxRoot, AssignmentExpressionSyntax assignment)
        {
            ExpressionSyntax setCurrentValue;
            if (!SetCurrentValueExpression.TryCreate(assignment, out setCurrentValue))
            {
                return context.Document;
            }

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            ArgumentSyntax property;
            if (!Arguments.TryCreateProperty(assignment, semanticModel, context.CancellationToken, out property))
            {
                return context.Document;
            }

            ArgumentSyntax value;
            if (!Arguments.TryCreateValue(assignment, semanticModel, context.CancellationToken, out value))
            {
                return context.Document;
            }

            var setCurrentValueInvocation = SyntaxFactory.InvocationExpression(setCurrentValue, SyntaxFactory.ArgumentList().AddArguments(property, value));
            var updated = syntaxRoot.ReplaceNode(assignment, setCurrentValueInvocation);
            return context.Document.WithSyntaxRoot(updated);
        }

        private static async Task<Document> ApplyFixAsync(CodeFixContext context, SyntaxNode syntaxRoot, InvocationExpressionSyntax invocation)
        {
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                             .ConfigureAwait(false);
            ArgumentSyntax property;
            ArgumentSyntax value;
            if (!invocation.TryGetSetValueArguments(semanticModel, context.CancellationToken, out property, out value))
            {
                return context.Document;
            }

            ExpressionSyntax setCurrentValue;
            if (SetCurrentValueExpression.TryCreate(invocation, out setCurrentValue))
            {
                var updated = syntaxRoot.ReplaceNode(invocation.Expression, setCurrentValue);
                return context.Document.WithSyntaxRoot(updated);
            }

            return context.Document;
        }

        private static class SetCurrentValueExpression
        {
            private static readonly IdentifierNameSyntax SetCurrentValueIdentifier = SyntaxFactory.IdentifierName(Names.SetCurrentValue);

            internal static bool TryCreate(AssignmentExpressionSyntax assignment, out ExpressionSyntax result)
            {
                return TryCreate(assignment.Left, out result);
            }

            internal static bool TryCreate(InvocationExpressionSyntax invocation, out ExpressionSyntax result)
            {
                return TryCreate(invocation.Expression, out result);
            }

            private static bool TryCreate(ExpressionSyntax expression, out ExpressionSyntax result)
            {
                result = null;
                var memberAccess = expression as MemberAccessExpressionSyntax;
                if (memberAccess != null)
                {
                    if (memberAccess.Expression.IsKind(SyntaxKind.ThisExpression))
                    {
                        result = memberAccess.WithName(SetCurrentValueIdentifier);
                        return true;
                    }

                    result = memberAccess.WithName(SetCurrentValueIdentifier);
                    return true;
                }

                if (expression.IsKind(SyntaxKind.IdentifierName))
                {
                    result = SetCurrentValueIdentifier;
                    return true;
                }

                return false;
            }
        }

        private static class Arguments
        {
            internal static bool TryCreateProperty(AssignmentExpressionSyntax assignment, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax result)
            {
                result = null;
                var property = semanticModel.GetSymbolInfo(assignment.Left, cancellationToken).Symbol as IPropertySymbol;

                IFieldSymbol fieldSymbol;
                if (property.TryGetMutableDependencyPropertyField(semanticModel, cancellationToken, out fieldSymbol))
                {
                    var minimalDisplayString = fieldSymbol.ToArgumentString(
                        semanticModel,
                        assignment.SpanStart);
                    var expressionSyntax = SyntaxFactory.ParseExpression(minimalDisplayString);
                    result = SyntaxFactory.Argument(expressionSyntax);
                }

                return result != null;
            }

            internal static bool TryCreateValue(AssignmentExpressionSyntax assignment, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax result)
            {
                var assignedType = semanticModel.GetTypeInfo(assignment.Left)
                                              .Type;
                if (assignedType == null)
                {
                    result = null;
                    return false;
                }

                var valueExpression = assignment.Right;
                if (assignedType.IsRepresentationConservingConversion(valueExpression, semanticModel, cancellationToken))
                {
                    result = SyntaxFactory.Argument(valueExpression);
                }
                else
                {
                    result = SyntaxFactory.Argument(
                            SyntaxFactory.CastExpression(
                                SyntaxFactory.ParseTypeName(
                                    assignedType.ToMinimalDisplayString(semanticModel, 0)),
                                    valueExpression));
                }

                return result != null;
            }
        }
    }
}
