namespace WpfAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Callback
    {
        internal static bool TryGetTarget(ArgumentSyntax callback, QualifiedType handlerType, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out IdentifierNameSyntax? identifier, [NotNullWhen(true)] out IMethodSymbol? method)
        {
            identifier = null;
            method = null;

            if (callback is null)
            {
                return false;
            }

            switch (callback.Expression)
            {
                case IdentifierNameSyntax identifierName
                    when semanticModel.TryGetSymbol(identifierName, cancellationToken, out method):
                    identifier = identifierName;
                    return true;
                case MemberAccessExpressionSyntax { Name: IdentifierNameSyntax candidate } memberAccess
                    when memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                         semanticModel.TryGetSymbol(candidate, cancellationToken, out method):
                    identifier = candidate;
                    return true;
                case LambdaExpressionSyntax { Body: InvocationExpressionSyntax invocation }:
                    switch (invocation.Expression)
                    {
                        case IdentifierNameSyntax identifierName
                            when semanticModel.TryGetSymbol(identifierName, cancellationToken, out method):
                            identifier = identifierName;
                            return true;
                        case MemberAccessExpressionSyntax { Name: IdentifierNameSyntax identifierName }
                            when semanticModel.TryGetSymbol(identifierName, cancellationToken, out method):
                            identifier = identifierName;
                            return true;
                    }

                    break;
            }

            return callback.Expression is ObjectCreationExpressionSyntax { ArgumentList: { Arguments: { Count: 1 } arguments } } creation &&
                   semanticModel.GetTypeInfoSafe(creation, cancellationToken).Type == handlerType &&
                   arguments.TrySingle(out var arg) &&
                   TryGetTarget(arg, handlerType, semanticModel, cancellationToken, out identifier, out method);
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

        internal static bool IsInvokedOnce(this IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            using var walker = InvocationWalker.InContainingClass(method, semanticModel, cancellationToken);
            return walker.IdentifierNames.Count == 1;
        }
    }
}
