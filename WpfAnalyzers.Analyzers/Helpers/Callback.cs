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
                    if (creation.ArgumentList.Arguments.TryGetSingle(out ArgumentSyntax arg))
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
                var dependencyProperty = semanticModel.GetSymbolSafe(memberAccess.Expression, cancellationToken) as IFieldSymbol;
                if (dependencyProperty?.Type != KnownSymbol.DependencyProperty)
                {
                    return false;
                }

                return DependencyProperty.TryGetRegisteredName(dependencyProperty, semanticModel, cancellationToken, out registeredName);
            }

            if (method == KnownSymbol.DependencyProperty.Register ||
                method == KnownSymbol.DependencyProperty.RegisterReadOnly ||
                method == KnownSymbol.DependencyProperty.RegisterAttached ||
                method == KnownSymbol.DependencyProperty.RegisterAttachedReadOnly)
            {
                var fieldDeclaration = callback.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
                if (fieldDeclaration == null)
                {
                    return false;
                }

                var dependencyProperty = semanticModel.GetDeclaredSymbolSafe(fieldDeclaration, cancellationToken) as IFieldSymbol;
                if (dependencyProperty == null)
                {
                    return false;
                }

                return DependencyProperty.TryGetRegisteredName(dependencyProperty, semanticModel, cancellationToken, out registeredName);
            }

            return false;
        }
    }
}