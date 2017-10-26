namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class Callback
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

            if (callback.Expression is ObjectCreationExpressionSyntax creation)
            {
                if (semanticModel.GetTypeInfoSafe(creation, cancellationToken).Type == callbackSymbol)
                {
                    if (creation.ArgumentList.Arguments.TryGetSingle(out var arg))
                    {
                        return TryGetName(arg, callbackSymbol, semanticModel, cancellationToken, out nameExpression, out name);
                    }
                }
            }

            return false;
        }

        internal static bool TryGetRegisteredName(ArgumentSyntax callback, SemanticModel semanticModel, CancellationToken cancellationToken, out string registeredName)
        {
            registeredName = null;
            var invocation = callback?.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocation == null)
            {
                return false;
            }

            if (DependencyProperty.TryGetRegisterCall(invocation, semanticModel, cancellationToken, out _) ||
                DependencyProperty.TryGetRegisterReadOnlyCall(invocation, semanticModel, cancellationToken, out _) ||
                DependencyProperty.TryGetRegisterAttachedCall(invocation, semanticModel, cancellationToken, out _) ||
                DependencyProperty.TryGetRegisterAttachedReadOnlyCall(invocation, semanticModel, cancellationToken, out _))
            {
                var nameArg = invocation.ArgumentList?.Arguments.FirstOrDefault();
                return nameArg?.TryGetStringValue(semanticModel, cancellationToken, out registeredName) == true;
            }

            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                (DependencyProperty.TryGetAddOwnerCall(invocation, semanticModel, cancellationToken, out _) ||
                 DependencyProperty.TryGetOverrideMetadataCall(invocation, semanticModel, cancellationToken, out _)))
            {
                if (BackingFieldOrProperty.TryCreate(semanticModel.GetSymbolSafe(memberAccess.Expression, cancellationToken), out var fieldOrProperty) ||
                    fieldOrProperty.Type == KnownSymbol.DependencyProperty)
                {
                    return DependencyProperty.TryGetRegisteredName(fieldOrProperty, semanticModel, cancellationToken, out registeredName);
                }

                return false;
            }

            return false;
        }
    }
}