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
            var memberAccess = invocation?.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null)
            {
                return false;
            }

            var method = semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol;
            if (method == KnownSymbol.DependencyProperty.OverrideMetadata ||
                method == KnownSymbol.DependencyProperty.AddOwner)
            {
                if (BackingFieldOrProperty.TryCreate(semanticModel.GetSymbolSafe(memberAccess.Expression, cancellationToken), out var fieldOrProperty) ||
                    fieldOrProperty.Type == KnownSymbol.DependencyProperty)
                {
                    return DependencyProperty.TryGetRegisteredName(fieldOrProperty, semanticModel, cancellationToken, out registeredName);
                }

                return false;
            }

            if (method == KnownSymbol.DependencyProperty.Register ||
                method == KnownSymbol.DependencyProperty.RegisterReadOnly ||
                method == KnownSymbol.DependencyProperty.RegisterAttached ||
                method == KnownSymbol.DependencyProperty.RegisterAttachedReadOnly)
            {
                return BackingFieldOrProperty.TryCreate(semanticModel.GetSymbolSafe(callback.FirstAncestorOrSelf<MemberDeclarationSyntax>(), cancellationToken), out var fieldOrProperty) &&
                       DependencyProperty.TryGetRegisteredName(fieldOrProperty, semanticModel, cancellationToken, out registeredName);
            }

            return false;
        }
    }
}