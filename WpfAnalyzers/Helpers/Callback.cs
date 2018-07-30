namespace WpfAnalyzers
{
    using System;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Callback
    {
        internal static bool TryGetTarget(ArgumentSyntax callback, QualifiedType callbackSymbol, SemanticModel semanticModel, CancellationToken cancellationToken, out IdentifierNameSyntax identifier, out IMethodSymbol method)
        {
            identifier = null;
            method = null;

            if (callback == null)
            {
                return false;
            }

            if (callback.Expression is IdentifierNameSyntax candidate &&
                semanticModel.TryGetSymbol(candidate, cancellationToken, out method))
            {
                identifier = candidate;
                return true;
            }

            return callback.Expression is ObjectCreationExpressionSyntax creation &&
                   semanticModel.GetTypeInfoSafe(creation, cancellationToken).Type == callbackSymbol &&
                   creation.ArgumentList.Arguments.TrySingle(out var arg) &&
                   TryGetTarget(arg, callbackSymbol, semanticModel, cancellationToken, out identifier, out method);
        }

        [Obsolete("Don't use this.")]
        internal static bool TryGetName(ArgumentSyntax callback, QualifiedType callbackSymbol, SemanticModel semanticModel, CancellationToken cancellationToken, out IdentifierNameSyntax nameExpression, out string name)
        {
            if (TryGetTarget(callback, callbackSymbol, semanticModel, cancellationToken, out nameExpression, out var target))
            {
                name = target.Name;
                return true;
            }

            name = null;
            return false;
        }

        internal static bool IsSingleExpression(MethodDeclarationSyntax method)
        {
            if (method.ExpressionBody != null)
            {
                return true;
            }

            return method.Body is BlockSyntax body &&
                   body.Statements.TrySingle(out var statement) &&
                   statement is ExpressionStatementSyntax;
        }
    }
}
