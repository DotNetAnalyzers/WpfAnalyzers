namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyChangedCallback
    {
        internal static bool TryGetName(ArgumentSyntax callback, SemanticModel semanticModel, CancellationToken cancellationToken, out IdentifierNameSyntax identifier, out string name)
        {
            return Callback.TryGetName(
                callback,
                KnownSymbol.PropertyChangedCallback,
                semanticModel,
                cancellationToken,
                out identifier,
                out name);
        }
    }
}