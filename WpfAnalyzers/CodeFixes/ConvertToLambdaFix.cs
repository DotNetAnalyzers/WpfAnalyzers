namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.CodeFixExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConvertToLambdaFix))]
    [Shared]
    internal class ConvertToLambdaFix : DocumentEditorCodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(Descriptors.WPF0023ConvertToLambda.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                                   .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ArgumentSyntax? argument))
                {
                    switch (argument.Expression)
                    {
                        case IdentifierNameSyntax identifier:
                            context.RegisterCodeFix("Convert to lambda", (editor, token) => ConvertToLambda(editor, identifier, token), "Convert to lambda", diagnostic);
                            break;
                        case ObjectCreationExpressionSyntax objectCreation when
                            objectCreation.TrySingleArgument(out argument) &&
                            argument.Expression is IdentifierNameSyntax identifier:
                            context.RegisterCodeFix("Convert to lambda", (editor, token) => ConvertToLambda(editor, identifier, token), "Convert to lambda", diagnostic);
                            break;
                        case LambdaExpressionSyntax lambda:
                            context.RegisterCodeFix("Convert to lambda", (editor, token) => ConvertToLambda(editor, lambda, token), "Convert to lambda", diagnostic);
                            break;
                        case ObjectCreationExpressionSyntax objectCreation when
                            objectCreation.TrySingleArgument(out argument) &&
                            argument.Expression is LambdaExpressionSyntax lambda:
                            context.RegisterCodeFix("Convert to lambda", (editor, token) => ConvertToLambda(editor, lambda, token), "Convert to lambda", diagnostic);
                            break;
                    }
                }
            }
        }

        private static void ConvertToLambda(DocumentEditor editor, IdentifierNameSyntax identifier, CancellationToken cancellationToken)
        {
            if (editor.SemanticModel.TryGetSymbol(identifier, cancellationToken, out IMethodSymbol? method) &&
                method.TrySingleMethodDeclaration(cancellationToken, out var declaration))
            {
                ConvertToLambda(editor, identifier, method, declaration, cancellationToken);
            }
        }

        private static void ConvertToLambda(DocumentEditor editor, LambdaExpressionSyntax lambda, CancellationToken cancellationToken)
        {
            if (editor.SemanticModel.TryGetSymbol(lambda.Body, cancellationToken, out IMethodSymbol? method) &&
                method.TrySingleMethodDeclaration(cancellationToken, out var declaration))
            {
                ConvertToLambda(editor, lambda, method, declaration, cancellationToken);
            }
        }

        private static void ConvertToLambda(DocumentEditor editor, SyntaxNode toReplace, IMethodSymbol method, MethodDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            if (method.Parameters.TrySingle(out var parameter))
            {
                if (declaration.ExpressionBody is { } expressionBody)
                {
                    editor.ReplaceNode(
                        toReplace,
                        SyntaxFactory.ParseExpression($"{parameter.Name} => {expressionBody.Expression}")
                                     .WithLeadingTriviaFrom(toReplace));
                    RemoveMethod(editor, method, declaration, cancellationToken);
                }
                else if (declaration.Body is BlockSyntax body &&
                         body.Statements.TrySingle(out var statement) &&
                         statement is ReturnStatementSyntax returnStatement)
                {
                    editor.ReplaceNode(
                        toReplace,
                        SyntaxFactory.ParseExpression($"{parameter.Name} => {returnStatement.Expression}")
                                     .WithLeadingTriviaFrom(toReplace));
                    RemoveMethod(editor, method, declaration, cancellationToken);
                }
            }

            if (method.Parameters.Length == 2 &&
                method.Parameters.TryElementAt(0, out var parameter1) &&
                method.Parameters.TryElementAt(1, out var parameter2))
            {
                if (declaration.ExpressionBody is ArrowExpressionClauseSyntax expressionBody)
                {
                    editor.ReplaceNode(
                        toReplace,
                        SyntaxFactory.ParseExpression($"({parameter1.Name}, {parameter2.Name}) => {expressionBody.Expression}")
                                     .WithLeadingTriviaFrom(toReplace));
                    RemoveMethod(editor, method, declaration, cancellationToken);
                }
                else if (declaration.Body is BlockSyntax body &&
                         body.Statements.TrySingle(out var statement))
                {
                    switch (statement)
                    {
                        case ReturnStatementSyntax returnStatement:
                            editor.ReplaceNode(
                                toReplace,
                                SyntaxFactory.ParseExpression($"({parameter1.Name}, {parameter2.Name}) => {returnStatement.Expression}")
                                             .WithLeadingTriviaFrom(toReplace));
                            RemoveMethod(editor, method, declaration, cancellationToken);
                            break;
                        case ExpressionStatementSyntax expressionStatement:
                            editor.ReplaceNode(
                                toReplace,
                                SyntaxFactory.ParseExpression($"({parameter1.Name}, {parameter2.Name}) => {expressionStatement.Expression}")
                                             .WithLeadingTriviaFrom(toReplace));
                            RemoveMethod(editor, method, declaration, cancellationToken);
                            break;
                    }
                }
            }
        }

        private static void RemoveMethod(DocumentEditor editor, IMethodSymbol method, MethodDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            if (method.DeclaredAccessibility == Accessibility.Private &&
                method.IsInvokedOnce(editor.SemanticModel, cancellationToken))
            {
                editor.RemoveNode(declaration);
            }
        }
    }
}
