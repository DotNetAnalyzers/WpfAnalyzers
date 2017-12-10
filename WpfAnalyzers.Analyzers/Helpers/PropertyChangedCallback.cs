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

        internal static bool TryGetRegisteredName(ArgumentSyntax callback, SemanticModel semanticModel, CancellationToken cancellationToken, out string registeredName)
        {
            registeredName = null;
            return PropertyMetadata.TryFindObjectCreationAncestor(callback, semanticModel, cancellationToken, out var objectCreation) &&
                   PropertyMetadata.TryGetRegisteredName(objectCreation, semanticModel, cancellationToken, out registeredName);
        }

        internal static bool IsPropertyChangedCallback(MethodDeclarationSyntax methodDeclaration, SemanticModel contextSemanticModel, CancellationToken contextCancellationToken, out BackingFieldOrProperty fieldOrProperty)
        {
            throw new System.NotImplementedException();
        }
    }
}