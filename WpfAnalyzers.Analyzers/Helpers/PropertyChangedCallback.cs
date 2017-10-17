namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class PropertyChangedCallback
    {
        internal static bool TryGetName(ArgumentSyntax callback, SemanticModel semanticModel, CancellationToken cancellationToken, out IdentifierNameSyntax nameExpression, out string name)
        {
            return Callback.TryGetName(
                callback,
                KnownSymbol.PropertyChangedCallback,
                semanticModel,
                cancellationToken,
                out nameExpression,
                out name);
        }

        internal static bool TryGetRegisteredName(ArgumentSyntax callback, SemanticModel semanticModel, CancellationToken cancellationToken, out string registeredName)
        {
            return Callback.TryGetRegisteredName(callback, semanticModel, cancellationToken, out registeredName);
        }
    }
}