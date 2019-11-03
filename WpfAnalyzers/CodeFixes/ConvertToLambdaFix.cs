namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Diagnostics.CodeAnalysis;
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
                SymbolAndDeclaration.TryCreate(method, cancellationToken, out SymbolAndDeclaration<IMethodSymbol, MethodDeclarationSyntax> symbolAndDeclaration) &&
                TryGetExpression(symbolAndDeclaration.Declaration, out var expression))
            {
                switch (method.Parameters.Length)
                {
                    case 1
                        when method.Parameters[0] is { } parameter:
                        editor.ReplaceNode(
                            identifier,
                            x => SyntaxFactory.ParseExpression($"{parameter.Name} => {expression}")
                                         .WithLeadingTriviaFrom(x));
                        RemoveIfNotUsed(editor, symbolAndDeclaration, cancellationToken);
                        break;
                    case 2
                        when method.Parameters[0] is { } parameter1 &&
                             method.Parameters[1] is { } parameter2:
                        editor.ReplaceNode(
                            identifier,
                            x => SyntaxFactory.ParseExpression($"({parameter1.Name}, {parameter2.Name}) => {expression}")
                                              .WithLeadingTriviaFrom(x));
                        RemoveIfNotUsed(editor, symbolAndDeclaration, cancellationToken);
                        break;
                }
            }
        }

        private static void ConvertToLambda(DocumentEditor editor, LambdaExpressionSyntax lambda, CancellationToken cancellationToken)
        {
            if (lambda is { Body: ExpressionSyntax body } &&
                editor.SemanticModel.TryGetSymbol(body, cancellationToken, out IMethodSymbol? method) &&
                SymbolAndDeclaration.TryCreate(method, cancellationToken, out SymbolAndDeclaration<IMethodSymbol, MethodDeclarationSyntax> symbolAndDeclaration) &&
                TryGetExpression(symbolAndDeclaration.Declaration, out var expression))
            {
                switch (method.Parameters.Length)
                {
                    case 1
                        when method.Parameters[0] is { } parameter:
                        editor.ReplaceNode(
                            lambda,
                            x => SyntaxFactory.ParseExpression($"{parameter.Name} => {expression}")
                                              .WithLeadingTriviaFrom(x));
                        RemoveIfNotUsed(editor, symbolAndDeclaration, cancellationToken);
                        break;
                    case 2
                        when method.Parameters[0] is { } parameter1 &&
                             method.Parameters[1] is { } parameter2:
                        editor.ReplaceNode(
                            lambda,
                            x => SyntaxFactory.ParseExpression($"({parameter1.Name}, {parameter2.Name}) => {expression}")
                                              .WithLeadingTriviaFrom(x));
                        RemoveIfNotUsed(editor, symbolAndDeclaration, cancellationToken);
                        break;
                }
            }
        }

        private static bool TryGetExpression(MethodDeclarationSyntax declaration, [NotNullWhen(true)] out ExpressionSyntax? expression)
        {
            if (declaration.ExpressionBody is { } expressionBody)
            {
                expression = expressionBody.Expression;
                return true;
            }

            if (declaration.Body is { Statements: { Count: 1 } statements } &&
                statements.TrySingle(out var statement))
            {
                switch (statement)
                {
                    case ReturnStatementSyntax { Expression: { } temp }:
                        expression = temp;
                        return true;
                    case ExpressionStatementSyntax { Expression: { } temp }:
                        expression = temp;
                        return true;
                }
            }

            expression = null;
            return false;
        }

        private static void RemoveIfNotUsed(DocumentEditor editor, SymbolAndDeclaration<IMethodSymbol, MethodDeclarationSyntax> symbolAndDeclaration, CancellationToken cancellationToken)
        {
            if (symbolAndDeclaration.Symbol.DeclaredAccessibility == Accessibility.Private &&
                symbolAndDeclaration.Symbol.IsInvokedOnce(editor.SemanticModel, cancellationToken))
            {
                editor.RemoveNode(symbolAndDeclaration.Declaration);
            }
        }
    }
}
