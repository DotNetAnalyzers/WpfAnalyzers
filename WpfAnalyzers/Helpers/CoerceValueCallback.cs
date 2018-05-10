namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class CoerceValueCallback
    {
        internal static bool TryGetName(ArgumentSyntax callback, SemanticModel semanticModel, CancellationToken cancellationToken, out IdentifierNameSyntax nameExpression, out string name)
        {
            return Callback.TryGetName(
                callback,
                KnownSymbol.CoerceValueCallback,
                semanticModel,
                cancellationToken,
                out nameExpression,
                out name);
        }
    }
}