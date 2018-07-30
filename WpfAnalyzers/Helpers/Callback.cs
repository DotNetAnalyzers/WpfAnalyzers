namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Callback
    {
        internal static bool TryGetTarget(ArgumentSyntax callback, QualifiedType handlerType, SemanticModel semanticModel, CancellationToken cancellationToken, out IdentifierNameSyntax identifier, out IMethodSymbol method)
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
                case LambdaExpressionSyntax candidate when
                    candidate.Body is InvocationExpressionSyntax invocation &&
                    invocation.Expression is IdentifierNameSyntax identifierName &&
                    semanticModel.TryGetSymbol(identifierName, cancellationToken, out method):
                {
                    identifier = identifierName;
                    return true;
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

            return method.Body is BlockSyntax body &&
                   body.Statements.TrySingle(out var statement) &&
                   (statement is ExpressionStatementSyntax ||
                    statement is ReturnStatementSyntax);
        }
    }
}
