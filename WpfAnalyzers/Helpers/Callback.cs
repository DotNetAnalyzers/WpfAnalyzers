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

            if (callback == null)
            {
                return false;
            }

            switch (callback.Expression)
            {
                case IdentifierNameSyntax identifierName when semanticModel.TryGetSymbol(identifierName, cancellationToken, out method):
                    identifier = identifierName;
                    return true;
                case MemberAccessExpressionSyntax memberAccess when
                    memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                    memberAccess.Name is IdentifierNameSyntax candidate &&
                    semanticModel.TryGetSymbol(candidate, cancellationToken, out method):
                    identifier = candidate;
                    return true;
                case LambdaExpressionSyntax candidate when
                    candidate.Body is InvocationExpressionSyntax invocation:
                    {
                        switch (invocation.Expression)
                        {
                            case IdentifierNameSyntax identifierName when semanticModel.TryGetSymbol(identifierName, cancellationToken, out method):
                                identifier = identifierName;
                                return true;
                            case MemberAccessExpressionSyntax memberAccess when
                                memberAccess.Name is IdentifierNameSyntax identifierName &&
                                semanticModel.TryGetSymbol(identifierName, cancellationToken, out method):
                                {
                                    identifier = identifierName;
                                    return true;
                                }
                        }

                        break;
                    }
            }

            return callback.Expression is ObjectCreationExpressionSyntax creation &&
                   semanticModel.GetTypeInfoSafe(creation, cancellationToken).Type == handlerType &&
                   creation.ArgumentList.Arguments.TrySingle(out var arg) &&
                   TryGetTarget(arg, handlerType, semanticModel, cancellationToken, out identifier, out method);
        }

        internal static bool IsSingleExpression(MethodDeclarationSyntax method)
        {
            if (method.ExpressionBody != null)
            {
                return true;
            }

            return method.Body is { } body &&
                   body.Statements.TrySingle(out var statement) &&
                   (statement is ExpressionStatementSyntax ||
                    statement is ReturnStatementSyntax);
        }

        internal static bool IsInvokedOnce(this IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            using (var walker = InvocationWalker.InContainingClass(method, semanticModel, cancellationToken))
            {
                return walker.IdentifierNames.Count == 1;
            }
        }
    }
}
