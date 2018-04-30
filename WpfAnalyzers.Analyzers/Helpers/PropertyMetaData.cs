namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyMetadata
    {
        internal static bool TryGetConstructor(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol constructor)
        {
            return semanticModel.TryGetSymbol(objectCreation, KnownSymbol.PropertyMetadata, cancellationToken, out constructor) ||
                   semanticModel.TryGetSymbol(objectCreation, KnownSymbol.UIPropertyMetadata, cancellationToken, out constructor) ||
                   semanticModel.TryGetSymbol(objectCreation, KnownSymbol.FrameworkPropertyMetadata, cancellationToken, out constructor);
        }

        internal static bool TryGetDefaultValue(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax defaultValueArg)
        {
            defaultValueArg = null;
            if (objectCreation?.ArgumentList == null ||
                objectCreation.ArgumentList.Arguments.Count == 0)
            {
                return false;
            }

            return TryGetConstructor(objectCreation, semanticModel, cancellationToken, out var constructor) &&
                   constructor.Parameters.TryFirst(out var parameter) &&
                   parameter.Type == KnownSymbol.Object &&
                   objectCreation.ArgumentList.Arguments.TryFirst(out defaultValueArg);
        }

        internal static bool TryGetPropertyChangedCallback(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax callback)
        {
            return TryGetCallback(objectCreation, KnownSymbol.PropertyChangedCallback, semanticModel, cancellationToken, out callback);
        }

        internal static bool TryGetCoerceValueCallback(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax callback)
        {
            return TryGetCallback(objectCreation, KnownSymbol.CoerceValueCallback, semanticModel, cancellationToken, out callback);
        }

        internal static bool TryGetRegisteredName(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, CancellationToken cancellationToken, out string registeredName)
        {
            registeredName = null;
            return TryGetConstructor(objectCreation, semanticModel, cancellationToken, out _) &&
                   DependencyProperty.TryGetRegisteredName(objectCreation?.FirstAncestorOrSelf<InvocationExpressionSyntax>(), semanticModel, cancellationToken, out registeredName);
        }

        internal static bool TryGetDependencyProperty(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty fieldOrProperty)
        {
            fieldOrProperty = default(BackingFieldOrProperty);
            var invocation = objectCreation.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocation == null)
            {
                return false;
            }

            if (DependencyProperty.TryGetRegisterCall(invocation, semanticModel, cancellationToken, out _) ||
                DependencyProperty.TryGetRegisterReadOnlyCall(invocation, semanticModel, cancellationToken, out _) ||
                DependencyProperty.TryGetRegisterAttachedCall(invocation, semanticModel, cancellationToken, out _) ||
                DependencyProperty.TryGetRegisterAttachedReadOnlyCall(invocation, semanticModel, cancellationToken, out _))
            {
                if (objectCreation.TryFirstAncestor<FieldDeclarationSyntax>(out var fieldDeclaration) &&
                    semanticModel.TryGetSymbol(fieldDeclaration, cancellationToken, out var field))
                {
                    return BackingFieldOrProperty.TryCreate(field, out fieldOrProperty);
                }

                if (objectCreation.TryFirstAncestor<PropertyDeclarationSyntax>(out var propertyDeclaration) &&
                    semanticModel.TryGetSymbol(propertyDeclaration, cancellationToken, out var property))
                {
                    return BackingFieldOrProperty.TryCreate(property, out fieldOrProperty);
                }

                return false;
            }

            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                (DependencyProperty.TryGetAddOwnerCall(invocation, semanticModel, cancellationToken, out _) ||
                 DependencyProperty.TryGetOverrideMetadataCall(invocation, semanticModel, cancellationToken, out _)) &&
                semanticModel.TryGetSymbol(memberAccess.Expression, cancellationToken, out ISymbol candidate))
            {
                return BackingFieldOrProperty.TryCreate(candidate, out fieldOrProperty);
            }

            return false;
        }

        private static bool TryGetCallback(ObjectCreationExpressionSyntax objectCreation, QualifiedType callbackType, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax callback)
        {
            callback = null;
            if (objectCreation?.ArgumentList == null ||
                objectCreation.ArgumentList.Arguments.Count == 0)
            {
                return false;
            }

            return TryGetConstructor(objectCreation, semanticModel, cancellationToken, out var constructor) &&
                   Argument.TryGetArgument(constructor.Parameters, objectCreation.ArgumentList, callbackType, out callback);
        }
    }
}
