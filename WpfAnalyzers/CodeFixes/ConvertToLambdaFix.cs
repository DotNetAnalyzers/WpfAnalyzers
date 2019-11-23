namespace WpfAnalyzers
{
    using System.Collections.Generic;
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

        private static void ConvertToLambda(DocumentEditor editor, IdentifierNameSyntax methodGroup, CancellationToken cancellationToken)
        {
            if (editor.SemanticModel.TryGetSymbol(methodGroup, cancellationToken, out IMethodSymbol? method) &&
                SymbolAndDeclaration.TryCreate(method, cancellationToken, out SymbolAndDeclaration<IMethodSymbol, MethodDeclarationSyntax> symbolAndDeclaration) &&
                TryGetExpression(symbolAndDeclaration.Declaration, out var expression))
            {
                switch (method)
                {
                    case { ReturnsVoid: false, Parameters: { Length: 1 } parameters }
                        when parameters[0] is { } parameter:
                        editor.ReplaceNode(
                            methodGroup,
                            (x, g) => g.ValueReturningLambdaExpression(parameter.Name, expression)
                                        .WithLeadingTriviaFrom(x));
                        RemoveIfNotUsed(editor, symbolAndDeclaration, cancellationToken);
                        break;
                    case { ReturnsVoid: true, Parameters: { Length: 2 } parameters }
                        when parameters[0] is { } parameter1 &&
                             parameters[1] is { } parameter2:
                        editor.ReplaceNode(
                            methodGroup,
                            (x, g) => g.VoidReturningLambdaExpression(
                                           new[] { g.LambdaParameter(parameter1.Name), g.LambdaParameter(parameter2.Name) },
                                           expression)
                                       .WithTriviaFrom(x));
                        RemoveIfNotUsed(editor, symbolAndDeclaration, cancellationToken);
                        break;
                    case { ReturnsVoid: false, Parameters: { Length: 2 } parameters }
                        when parameters[0] is { } parameter1 &&
                             parameters[1] is { } parameter2:
                        editor.ReplaceNode(
                            methodGroup,
                            (x, g) => g.ValueReturningLambdaExpression(
                                           new[] { g.LambdaParameter(parameter1.Name), g.LambdaParameter(parameter2.Name) },
                                           expression)
                                       .WithTriviaFrom(x));
                        RemoveIfNotUsed(editor, symbolAndDeclaration, cancellationToken);
                        break;
                }
            }
        }

        private static void ConvertToLambda(DocumentEditor editor, LambdaExpressionSyntax lambda, CancellationToken cancellationToken)
        {
            if (lambda is { Body: InvocationExpressionSyntax body } &&
                editor.SemanticModel.TryGetSymbol(body, cancellationToken, out var method) &&
                SymbolAndDeclaration.TryCreate(method, cancellationToken, out SymbolAndDeclaration<IMethodSymbol, MethodDeclarationSyntax> symbolAndDeclaration) &&
                TryGetExpression(symbolAndDeclaration.Declaration, out var expression))
            {
                switch (method)
                {
                    case { Parameters: { Length: 1 } parameters }
                        when parameters[0] is { } parameter &&
                             body.TryFindArgument(parameter, out var argument):
                        editor.ReplaceNode(
                            body,
                            x => ParameterRewriter.Rewrite(expression, new[] { (parameter, argument.Expression) }, editor.SemanticModel, cancellationToken)
                                                  .WithTriviaFrom(x));
                        RemoveIfNotUsed(editor, symbolAndDeclaration, cancellationToken);
                        break;
                    case { Parameters: { Length: 2 } parameters }
                        when parameters[0] is { } parameter1 &&
                             body.TryFindArgument(parameter1, out var argument1) &&
                             parameters[1] is { } parameter2 &&
                             body.TryFindArgument(parameter2, out var argument2):
                        editor.ReplaceNode(
                            body,
                            x => ParameterRewriter.Rewrite(expression, new[] { (parameter1, argument1.Expression), (parameter2, argument2.Expression) }, editor.SemanticModel, cancellationToken)
                                                  .WithTriviaFrom(x));
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

        private class ParameterRewriter : CSharpSyntaxRewriter
        {
            private readonly IReadOnlyList<(IParameterSymbol Parameter, ExpressionSyntax Expression)> replacements;
            private readonly SemanticModel semanticModel;
            private readonly CancellationToken cancellationToken;

            private ParameterRewriter(IReadOnlyList<(IParameterSymbol Parameter, ExpressionSyntax Expression)> replacements, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                this.replacements = replacements;
                this.semanticModel = semanticModel;
                this.cancellationToken = cancellationToken;
            }

            public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
            {
                foreach ((var parameter, var expression) in this.replacements)
                {
                    if (node.IsSymbol(parameter, this.semanticModel, this.cancellationToken))
                    {
                        return expression.WithTriviaFrom(node);
                    }
                }

                return base.VisitIdentifierName(node);
            }

            internal static ExpressionSyntax Rewrite(ExpressionSyntax expression, IReadOnlyList<(IParameterSymbol Parameter, ExpressionSyntax Expression)> replacements, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                return (ExpressionSyntax)new ParameterRewriter(replacements, semanticModel, cancellationToken)
                    .Visit(expression);
            }
        }
    }
}
