namespace WpfAnalyzers
{
    using System.Collections.Generic;
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
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(Descriptors.WPF0023ConvertToLambda.Id);

        protected override async Task RegisterCodeFixesAsync(DocumentEditorCodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                                   .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                if (syntaxRoot is { } &&
                    syntaxRoot.TryFindNodeOrAncestor(diagnostic, out ArgumentSyntax? argument))
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
                TryGetExpression(symbolAndDeclaration.Declaration, editor.SemanticModel) is { } expression)
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
                TryGetExpression(symbolAndDeclaration.Declaration, editor.SemanticModel) is { } expression)
            {
                switch (method)
                {
                    case { Parameters: { Length: 1 } parameters }
                        when parameters[0] is { } parameter &&
                             body.TryFindArgument(parameter, out var argument):
                        editor.ReplaceNode(
                            body,
                            x => ParameterRewriter.Rewrite(expression, new[] { (parameter.Name, argument.Expression) }, editor.SemanticModel)
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
                            x => ParameterRewriter.Rewrite(expression, new[] { (parameter1.Name, argument1.Expression), (parameter2.Name, argument2.Expression) }, editor.SemanticModel)
                                                  .WithTriviaFrom(x));
                        RemoveIfNotUsed(editor, symbolAndDeclaration, cancellationToken);
                        break;
                }
            }
        }

        private static ExpressionSyntax? TryGetExpression(MethodDeclarationSyntax declaration, SemanticModel semanticModel)
        {
            return declaration switch
            {
                { ExpressionBody: { Expression: { } expression } } => expression,
                { Body: { Statements: { Count: 1 } statements } } =>
                    statements[0] switch
                    {
                        ReturnStatementSyntax { Expression: { } expression } => expression,
                        ExpressionStatementSyntax { Expression: { } expression } => expression,
                        _ => null,
                    },
                { Body: { Statements: { Count: 2 } statements } }
                    when statements[0] is LocalDeclarationStatementSyntax { Declaration: { Variables: { Count: 1 } variables } } &&
                         variables[0] is { Identifier: { } identifier, Initializer: { Value: CastExpressionSyntax cast } }
                         =>
                    statements[1] switch
                    {
                        ReturnStatementSyntax { Expression: { } expression } => ParameterRewriter.Rewrite(expression, new[] { (identifier.ValueText, (ExpressionSyntax)SyntaxFactory.ParenthesizedExpression(cast)) }, semanticModel),
                        ExpressionStatementSyntax { Expression: { } expression } => ParameterRewriter.Rewrite(expression, new[] { (identifier.ValueText, (ExpressionSyntax)SyntaxFactory.ParenthesizedExpression(cast)) }, semanticModel),
                        _ => null,
                    },
                _ => null,
            };
        }

        private static void RemoveIfNotUsed(DocumentEditor editor, SymbolAndDeclaration<IMethodSymbol, MethodDeclarationSyntax> symbolAndDeclaration, CancellationToken cancellationToken)
        {
            if (symbolAndDeclaration.Symbol.DeclaredAccessibility == Accessibility.Private &&
                Callback.IsInvokedOnce(symbolAndDeclaration.Symbol, editor.SemanticModel, cancellationToken))
            {
                editor.RemoveNode(symbolAndDeclaration.Declaration);
            }
        }

        private class ParameterRewriter : CSharpSyntaxRewriter
        {
            private readonly IReadOnlyList<(string Symbol, ExpressionSyntax Expression)> replacements;
            private readonly SemanticModel semanticModel;

            private ParameterRewriter(IReadOnlyList<(string Symbol, ExpressionSyntax Expression)> replacements, SemanticModel semanticModel)
            {
                this.replacements = replacements;
                this.semanticModel = semanticModel;
            }

            public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
            {
                foreach ((var symbol, var expression) in this.replacements)
                {
                    if (node.Identifier.ValueText == symbol &&
                        this.semanticModel.GetSpeculativeSymbolInfo(node.SpanStart, node, SpeculativeBindingOption.BindAsExpression).Symbol is { } speculativeSymbol &&
                        speculativeSymbol.IsEitherKind(SymbolKind.Local, SymbolKind.Parameter))
                    {
                        return expression.WithTriviaFrom(node);
                    }
                }

                return base.VisitIdentifierName(node);
            }

            internal static ExpressionSyntax Rewrite(ExpressionSyntax expression, IReadOnlyList<(string Symbol, ExpressionSyntax Expression)> replacements, SemanticModel semanticModel)
            {
                return (ExpressionSyntax)new ParameterRewriter(replacements, semanticModel)
                    .Visit(expression);
            }
        }
    }
}
