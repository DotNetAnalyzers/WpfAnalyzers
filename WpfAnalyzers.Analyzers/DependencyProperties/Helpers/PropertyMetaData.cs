namespace WpfAnalyzers.DependencyProperties
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyMetaData
    {
        internal static bool TryGetConstructor(
            ObjectCreationExpressionSyntax objectCreation,
            SemanticModel semanticModel,
            CancellationToken cancellationToken,
            out IMethodSymbol constructor)
        {
            constructor = null;
            var createdType = semanticModel.GetTypeInfoSafe(objectCreation, cancellationToken)
                                           .Type;
            if (createdType == null)
            {
                return false;
            }

            if (!createdType.Is(KnownSymbol.PropertyMetadata) ||
                objectCreation?.ArgumentList.Arguments.FirstOrDefault() == null)
            {
                return false;
            }

            if (createdType.ContainingNamespace != KnownSymbol.PropertyMetadata.Namespace)
            {
                // don't think there is a way to handle custom subclassed.
                // should not be common
                return false;
            }

            constructor = semanticModel.GetSymbolSafe(objectCreation, cancellationToken) as IMethodSymbol;
            return constructor != null;
        }

        internal static bool TryGetDefaultValue(
            ObjectCreationExpressionSyntax objectCreation,
            SemanticModel semanticModel,
            CancellationToken cancellationToken,
            out ArgumentSyntax defaultValueArg)
        {
            defaultValueArg = null;
            if (objectCreation?.ArgumentList == null ||
                objectCreation.ArgumentList.Arguments.Count == 0)
            {
                return false;
            }

            if (!TryGetConstructor(objectCreation, semanticModel, cancellationToken, out IMethodSymbol constructor))
            {
                return false;
            }

            if (constructor == null ||
    !constructor.Parameters.TryGetFirst(out IParameterSymbol parameter) ||
    parameter.Type != KnownSymbol.Object)
            {
                return false;
            }

            return objectCreation.ArgumentList.Arguments.TryGetFirst(out defaultValueArg);
        }

        internal static bool TryGetPropertyChangedCallback(
            ObjectCreationExpressionSyntax objectCreation,
            SemanticModel semanticModel,
            CancellationToken cancellationToken,
            out ArgumentSyntax propertyChangedCallbackArg)
        {
            return TryGetCallback(
                objectCreation,
                KnownSymbol.PropertyChangedCallback,
                semanticModel,
                cancellationToken,
                out propertyChangedCallbackArg);
        }

        internal static bool TryGetCoerceValueCallback(
            ObjectCreationExpressionSyntax objectCreation,
            SemanticModel semanticModel,
            CancellationToken cancellationToken,
            out ArgumentSyntax propertyChangedCallbackArg)
        {
            return TryGetCallback(
                objectCreation,
                KnownSymbol.CoerceValueCallback,
                semanticModel,
                cancellationToken,
                out propertyChangedCallbackArg);
        }

        internal static bool TryGetDependencyProperty(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol dependencyProperty)
        {
            dependencyProperty = null;
            var declarator = objectCreation.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
            if (declarator == null)
            {
                return false;
            }

            dependencyProperty = semanticModel.GetDeclaredSymbolSafe(declarator, cancellationToken) as IFieldSymbol;
            return dependencyProperty != null;
        }

        internal static bool TryGetCallback(
            ObjectCreationExpressionSyntax objectCreation,
            QualifiedType callbackType,
            SemanticModel semanticModel,
            CancellationToken cancellationToken,
            out ArgumentSyntax callback)
        {
            callback = null;
            if (objectCreation?.ArgumentList == null ||
                objectCreation.ArgumentList.Arguments.Count == 0)
            {
                return false;
            }

            if (!TryGetConstructor(objectCreation, semanticModel, cancellationToken, out IMethodSymbol constructor))
            {
                return false;
            }

            for (int i = 0; i < constructor.Parameters.Length; i++)
            {
                var parameter = constructor.Parameters[i];
                if (parameter.Type == callbackType)
                {
                    callback = objectCreation.ArgumentList.Arguments[i];
                    return true;
                }
            }

            return false;
        }
    }
}
