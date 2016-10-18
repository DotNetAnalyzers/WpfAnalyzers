namespace WpfAnalyzers.DependencyProperties
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
            var identifierNameSyntax = callback.Expression as IdentifierNameSyntax;
            if (identifierNameSyntax != null)
            {
                nameExpression = identifierNameSyntax;
                name = identifierNameSyntax.Identifier.ValueText;
                return true;
            }

            var creation = callback.Expression as ObjectCreationExpressionSyntax;
            if (creation != null)
            {
                if (semanticModel.SemanticModelFor(creation).GetTypeInfo(creation, cancellationToken).Type == callbackSymbol)
                {
                    ArgumentSyntax arg;
                    if (creation.ArgumentList.Arguments.TryGetSingle(out arg))
                    {
                        return TryGetName(arg, callbackSymbol, semanticModel, cancellationToken, out nameExpression, out name);
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