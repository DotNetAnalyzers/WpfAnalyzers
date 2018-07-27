namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Callback
    {
        internal static bool TryGetName(ArgumentSyntax callback, QualifiedType callbackSymbol, SemanticModel semanticModel, CancellationToken cancellationToken, out IdentifierNameSyntax nameExpression, out string name)
        {
            nameExpression = null;
            name = null;

            if (callback == null)
            {
                return false;
            }

            if (callback.Expression is IdentifierNameSyntax identifierNameSyntax)
            {
                nameExpression = identifierNameSyntax;
                name = identifierNameSyntax.Identifier.ValueText;
                return true;
            }

            return callback.Expression is ObjectCreationExpressionSyntax creation &&
                   semanticModel.GetTypeInfoSafe(creation, cancellationToken).Type == callbackSymbol &&
                   creation.ArgumentList.Arguments.TrySingle(out var arg) &&
                   TryGetName(arg, callbackSymbol, semanticModel, cancellationToken, out nameExpression, out name);
        }
    }
}
