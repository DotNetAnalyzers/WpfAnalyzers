namespace WpfAnalyzers.DependencyProperties
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class PropertyChangedCallback
    {
        internal static bool TryGetName(ArgumentSyntax propertyChangedCallbackArg, SemanticModel semanticModel, CancellationToken cancellationToken, out IdentifierNameSyntax nameExpression, out string name)
        {
            nameExpression = null;
            name = null;
            var identifierNameSyntax = propertyChangedCallbackArg.Expression as IdentifierNameSyntax;
            if (identifierNameSyntax != null)
            {
                nameExpression = identifierNameSyntax;
                name = identifierNameSyntax.Identifier.ValueText;
                return true;
            }

            var creation = propertyChangedCallbackArg.Expression as ObjectCreationExpressionSyntax;
            if (creation != null)
            {
                if (semanticModel.SemanticModelFor(creation).GetTypeInfo(creation, cancellationToken).Type == KnownSymbol.PropertyChangedCallback)
                {
                    ArgumentSyntax arg;
                    if (creation.ArgumentList.Arguments.TryGetSingle(out arg))
                    {
                        return TryGetName(arg, semanticModel, cancellationToken, out nameExpression, out name);
                    }
                }
            }

            return false;
        }

        public static bool TryGetRegisteredName(ArgumentSyntax callback, SemanticModel semanticModel, CancellationToken cancellationToken, out string registeredName)
        {
            registeredName = null;
            if (callback == null)
            {
                return false;
            }

            var fieldDeclaration = callback.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
            var dependencyProperty = semanticModel.GetDeclaredSymbol(fieldDeclaration) as IFieldSymbol;
            return DependencyProperty.TryGetRegisteredName(dependencyProperty, semanticModel, cancellationToken, out registeredName);
        }
    }
}