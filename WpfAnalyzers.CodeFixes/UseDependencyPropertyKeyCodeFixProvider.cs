namespace WpfAnalyzers
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
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseDependencyPropertyKeyCodeFixProvider))]
    [Shared]
    internal class UseDependencyPropertyKeyCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(WPF0040SetUsingDependencyPropertyKey.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => null;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken)
                                              .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var invocation = syntaxRoot.FindNode(diagnostic.Location.SourceSpan)
                                           .FirstAncestorOrSelf<InvocationExpressionSyntax>();

                if (invocation == null || invocation.IsMissing)
                {
                    continue;
                }

                if (DependencyObject.TryGetSetValueCall(invocation, semanticModel, context.CancellationToken, out _) &&
                    BackingFieldOrProperty.TryCreate(semanticModel.GetSymbolSafe(invocation.ArgumentList.Arguments[0].Expression, context.CancellationToken), out var fieldOrProperty))
                {
                    if (DependencyProperty.TryGetDependencyPropertyKeyField(
                        fieldOrProperty,
                        semanticModel,
                        context.CancellationToken,
                        out var keyField))
                    {
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                invocation.ToString(),
                                cancellationToken => ApplyFixAsync(
                                    context.Document,
                                    invocation,
                                    null,
                                    keyField.CreateArgument(semanticModel, token.SpanStart),
                                    cancellationToken),
                                 this.GetType().FullName),
                            diagnostic);
                    }

                    continue;
                }

                if (DependencyObject.TryGetSetCurrentValueCall(invocation, semanticModel, context.CancellationToken, out _) &&
                    BackingFieldOrProperty.TryCreate(semanticModel.GetSymbolSafe(invocation.ArgumentList.Arguments[0].Expression, context.CancellationToken), out fieldOrProperty))
                {
                    if (DependencyProperty.TryGetDependencyPropertyKeyField(
                        fieldOrProperty,
                        semanticModel,
                        context.CancellationToken,
                        out var keyField))
                    {
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                invocation.ToString(),
                                cancellationToken => ApplyFixAsync(
                                    context.Document,
                                    invocation,
                                    SetValueExpression(invocation.Expression),
                                    keyField.CreateArgument(semanticModel, token.SpanStart),
                                    cancellationToken),
                                this.GetType().FullName),
                            diagnostic);
                    }
                }
            }
        }

        private static async Task<Document> ApplyFixAsync(Document document, InvocationExpressionSyntax oldNode, ExpressionSyntax expression, ArgumentSyntax argument, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            if (expression != null)
            {
                editor.ReplaceNode(oldNode.Expression, expression);
            }

            editor.ReplaceNode(oldNode.ArgumentList.Arguments[0], argument);
            return editor.GetChangedDocument();
        }

        private static ExpressionSyntax SetValueExpression(ExpressionSyntax old)
        {
            if (old is IdentifierNameSyntax identifierNameSyntax)
            {
                return identifierNameSyntax.Identifier.ValueText == "SetCurrentValue"
                           ? identifierNameSyntax.WithIdentifier(SyntaxFactory.Identifier("SetValue"))
                           : identifierNameSyntax;
            }

            if (old is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
            {
                var newName = SetValueExpression(memberAccessExpressionSyntax.Name) as SimpleNameSyntax;
                return memberAccessExpressionSyntax.WithName(newName);
            }

            return old;
        }
    }
}
