namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class CoerceValueCallback
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

        internal static bool TryGetRegisteredName(ArgumentSyntax callback, SemanticModel semanticModel, CancellationToken cancellationToken, out string registeredName)
        {
            registeredName = null;
            return PropertyMetadata.TryFindObjectCreationAncestor(callback, semanticModel, cancellationToken, out var objectCreation) &&
                   PropertyMetadata.TryGetRegisteredName(objectCreation, semanticModel, cancellationToken, out registeredName);
        }
    }
}