namespace WpfAnalyzers
{
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal readonly struct Callback
    {
        internal readonly IdentifierNameSyntax Identifier;
        internal readonly IMethodSymbol Target;

        internal Callback(IdentifierNameSyntax identifier, IMethodSymbol target)
        {
            this.Identifier = identifier;
            this.Target = target;
        }

        internal static Callback? Match(ArgumentSyntax callback, QualifiedType handlerType, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return callback.Expression switch
            {
                IdentifierNameSyntax identifierName
                   when FindMethod(identifierName) is { } method
                   => new Callback(identifierName, method),
                MemberAccessExpressionSyntax { Name: IdentifierNameSyntax identifierName }
                memberAccess
                    when memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                    FindMethod(identifierName) is { } method
                     => new Callback(identifierName, method),
                LambdaExpressionSyntax { Body: InvocationExpressionSyntax invocation } =>
                    invocation switch
                    {
                        { Expression: IdentifierNameSyntax identifierName }
                                when FindMethod(identifierName) is { } method
                        => new Callback(identifierName, method),
                        { Expression: MemberAccessExpressionSyntax { Name: IdentifierNameSyntax identifierName } }
                                when FindMethod(identifierName) is { } method
                        => new Callback(identifierName, method),
                        _ => null,
                    },

                ObjectCreationExpressionSyntax { ArgumentList: { Arguments: { Count: 1 } arguments } } creation
                    when creation.IsType(handlerType, semanticModel, cancellationToken)
                    => Match(arguments[0], handlerType, semanticModel, cancellationToken),
                _ => null,
            };

            IMethodSymbol? FindMethod(IdentifierNameSyntax candidate)
            {
                if (semanticModel.TryGetSymbol(candidate, cancellationToken, out var symbol) &&
                    symbol is IMethodSymbol method)
                {
                    return method;
                }

                return null;
            }
        }

        internal static bool CanInlineBody(MethodDeclarationSyntax method)
        {
            return method switch
            {
                { ExpressionBody: { } } => true,
                { Body: { Statements: { Count: 1 } statements } } => statements[0].IsEither(SyntaxKind.ExpressionStatement, SyntaxKind.ReturnStatement),
                { Body: { Statements: { Count: 2 } statements } }
                    when statements[0] is LocalDeclarationStatementSyntax { Declaration: { Variables: { Count: 1 } variables } } &&
                         variables[0].Initializer is { Value: CastExpressionSyntax _ }
                    => statements[1].IsEither(SyntaxKind.ExpressionStatement, SyntaxKind.ReturnStatement),
                _ => false,
            };
        }

        internal static bool IsInvokedOnce(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            using var walker = InvocationWalker.InContainingClass(method, semanticModel, cancellationToken);
            return walker.IdentifierNames.Count == 1;
        }
    }
}
