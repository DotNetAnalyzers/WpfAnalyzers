namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class PropertyChangedCallback
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

        internal static bool TryGetUsages(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out IReadOnlyList<InvocationExpressionSyntax> results)
        {
            results = null;
            if (methodDeclaration == null ||
                methodDeclaration.ParameterList == null ||
                methodDeclaration.ParameterList.Parameters.Count != 2 ||
                !methodDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                return false;
            }

            var parameters = methodDeclaration.ParameterList.Parameters;
            if (!parameters[0].Type.IsSameType(KnownSymbol.DependencyObject, semanticModel, cancellationToken) ||
                !parameters[1].Type.IsSameType(KnownSymbol.DependencyPropertyChangedEventArgs, semanticModel, cancellationToken))
            {
                return false;
            }

            List<InvocationExpressionSyntax> temp = null;
            using (var walker = IdentifierNameWalker.Borrow(methodDeclaration.Parent))
            {
                foreach (var candidate in walker.IdentifierNames)
                {
                    if (!ReferenceEquals(candidate.Parent, methodDeclaration) &&
                        candidate.Identifier.ValueText == methodDeclaration.Identifier.ValueText)
                    {
                        var candidateRegistration = candidate.FirstAncestorOrSelf<InvocationExpressionSyntax>();
                        if (DependencyProperty.TryGetRegisterCall(candidateRegistration, semanticModel, cancellationToken, out _) ||
                            DependencyProperty.TryGetRegisterReadOnlyCall(candidateRegistration, semanticModel, cancellationToken, out _) ||
                            DependencyProperty.TryGetRegisterAttachedCall(candidateRegistration, semanticModel, cancellationToken, out _) ||
                            DependencyProperty.TryGetRegisterAttachedReadOnlyCall(candidateRegistration, semanticModel, cancellationToken, out _) ||
                            DependencyProperty.TryGetAddOwnerCall(candidateRegistration, semanticModel, cancellationToken, out _) ||
                            DependencyProperty.TryGetOverrideMetadataCall(candidateRegistration, semanticModel, cancellationToken, out _))
                        {
                            if (temp == null)
                            {
                                temp = new List<InvocationExpressionSyntax>();
                            }

                            temp.Add(candidateRegistration);
                        }
                    }
                }
            }

            if (temp == null)
            {
                return false;
            }

            results = temp;
            return true;
        }
    }
}