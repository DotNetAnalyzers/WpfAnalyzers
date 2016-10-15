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

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var node = syntaxRoot.FindNode(diagnostic.Location.SourceSpan);
                if (node == null || node.IsMissing)
                {
                    continue;
                }

                var fix = await GetFixAsync(context, diagnostic).ConfigureAwait(false);
                if (!fix.IsEmpty)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            fix.SetCurrentValueCall.ToString(),
                            c => ApplyFixAsync(c, context.Document, fix),
                            nameof(UseSetCurrentValueCodeFixProvider)),
                        diagnostic);
                }
            }
        }

        private static async Task<Fix> GetFixAsync(CodeFixContext context, Diagnostic diagnostic)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);

            var assignment = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                       .FirstAncestorOrSelf<AssignmentExpressionSyntax>();
            if (assignment != null && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                                 .ConfigureAwait(false);
                InvocationExpressionSyntax setCurrentValueCall;
                if (TryGetFix(semanticModel, context.CancellationToken, assignment, out setCurrentValueCall))
                {
                    return new Fix(assignment, setCurrentValueCall);
                }
            }

            var invocation = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                       .FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocation != null)
            {
                var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                                 .ConfigureAwait(false);
                InvocationExpressionSyntax setCurrentValueCall;
                if (TryGetFix(semanticModel, context.CancellationToken, invocation, out setCurrentValueCall))
                {
                    return new Fix(invocation, setCurrentValueCall);
                }
            }

            return Fix.Empty;
        }

        private static bool TryGetFix(
            SemanticModel semanticModel,
            CancellationToken cancellationToken,
            AssignmentExpressionSyntax assignment,
            out InvocationExpressionSyntax setCurrentValueInvocation)
        {
            setCurrentValueInvocation = null;
            ExpressionSyntax setCurrentValue;
            if (!SetCurrentValueExpression.TryCreate(assignment, out setCurrentValue))
            {
                return false;
            }

            ArgumentSyntax property;
            if (!Arguments.TryCreateProperty(assignment, semanticModel, cancellationToken, out property))
            {
                return false;
            }

            ArgumentSyntax value;
            if (!Arguments.TryCreateValue(assignment, semanticModel, cancellationToken, out value))
            {
                return false;
            }

            setCurrentValueInvocation = SyntaxFactory.InvocationExpression(
                                                         setCurrentValue,
                                                         SyntaxFactory.ArgumentList()
                                                                      .AddArguments(property, value));
            return true;
        }

        private static bool TryGetFix(
            SemanticModel semanticModel,
            CancellationToken cancellationToken,
            InvocationExpressionSyntax setValue,
            out InvocationExpressionSyntax setCurrentValueInvocation)
        {
            setCurrentValueInvocation = null;
            ArgumentSyntax property;
            ArgumentSyntax value;
            IFieldSymbol setField;
            if (!DependencyObject.TryGetSetValueArguments(
                    setValue,
                    semanticModel,
                    cancellationToken,
                    out property,
                    out setField,
                    out value))
            {
                return false;
            }

            ExpressionSyntax setCurrentValue;
            if (SetCurrentValueExpression.TryCreate(setValue, out setCurrentValue))
            {
                setCurrentValueInvocation = setValue.WithExpression(setCurrentValue);
                return true;
            }

            return false;
        }

        private static async Task<Document> ApplyFixAsync(
            CancellationToken cancellationToken,
            Document document,
            Fix fix)
        {
            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            return document.WithSyntaxRoot(syntaxRoot.ReplaceNode(fix.OldNode, fix.SetCurrentValueCall));
        }

        private struct Fix
        {
            public static readonly Fix Empty = new Fix(null, null);

            public readonly ExpressionSyntax OldNode;
            public readonly InvocationExpressionSyntax SetCurrentValueCall;

            public Fix(ExpressionSyntax oldNode, InvocationExpressionSyntax setCurrentValueCall)
            {
                this.OldNode = oldNode;
                this.SetCurrentValueCall = setCurrentValueCall;
            }

            public bool IsEmpty => this.SetCurrentValueCall == null;
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

                var memberBinding = expression as MemberBindingExpressionSyntax;
                if (memberBinding != null)
                {
                    result = memberBinding.WithName(SetCurrentValueIdentifier);
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
                if (ClrProperty.TryGetSingleBackingField(property, semanticModel, cancellationToken, out fieldSymbol))
                {
                    result = DependencyProperty.CreateArgument(fieldSymbol, semanticModel, assignment.SpanStart);
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
                if (assignedType.IsRepresentationPreservingConversion(valueExpression, semanticModel, cancellationToken))
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
