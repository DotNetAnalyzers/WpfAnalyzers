namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseDependencyPropertyKeyFix))]
    [Shared]
    internal class UseDependencyPropertyKeyFix : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Descriptors.WPF0040SetUsingDependencyPropertyKey.Id);

        public override FixAllProvider? GetFixAllProvider() => null;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken)
                                           .ConfigureAwait(false);

            var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken)
                                              .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out InvocationExpressionSyntax? invocation) &&
                    invocation is { ArgumentList: { Arguments: { } arguments } } &&
                    arguments.FirstOrDefault() is { Expression: { } dp } &&
                    semanticModel.TryGetSymbol(dp, context.CancellationToken, out var symbol) &&
                    BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out var fieldOrProperty) &&
                    DependencyProperty.TryGetDependencyPropertyKeyFieldOrProperty(fieldOrProperty, semanticModel, context.CancellationToken, out var keyField))
                {
                    if (DependencyObject.TryGetSetValueCall(invocation, semanticModel, context.CancellationToken, out _))
                    {
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                invocation.ToString(),
                                cancellationToken => ApplyFixAsync(
                                    context.Document,
                                    invocation,
                                    null,
                                    keyField.CreateArgument(semanticModel, invocation.SpanStart),
                                    cancellationToken),
                                this.GetType().FullName),
                            diagnostic);
                    }

                    if (DependencyObject.TryGetSetCurrentValueCall(invocation, semanticModel, context.CancellationToken, out _))
                    {
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                invocation.ToString(),
                                cancellationToken => ApplyFixAsync(
                                    context.Document,
                                    invocation,
                                    SetValueExpression(invocation.Expression),
                                    keyField.CreateArgument(semanticModel, invocation.SpanStart),
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
