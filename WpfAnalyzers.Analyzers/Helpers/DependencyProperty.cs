namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class DependencyProperty
    {
        internal static bool TryGetRegisterCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return TryGetCall(
                invocation,
                KnownSymbol.DependencyProperty.Register,
                semanticModel,
                cancellationToken,
                out method);
        }

        internal static bool TryGetRegisterReadOnlyCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return TryGetCall(
                invocation,
                KnownSymbol.DependencyProperty.RegisterReadOnly,
                semanticModel,
                cancellationToken,
                out method);
        }

        internal static bool TryGetRegisterAttachedCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return TryGetCall(
                invocation,
                KnownSymbol.DependencyProperty.RegisterAttached,
                semanticModel,
                cancellationToken,
                out method);
        }

        internal static bool TryGetRegisterAttachedReadOnlyCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return TryGetCall(
                invocation,
                KnownSymbol.DependencyProperty.RegisterAttachedReadOnly,
                semanticModel,
                cancellationToken,
                out method);
        }

        internal static bool TryGetAddOwnerCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return TryGetCall(
                invocation,
                KnownSymbol.DependencyProperty.AddOwner,
                semanticModel,
                cancellationToken,
                out method);
        }

        internal static bool TryGetOverrideMetadataCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return TryGetCall(
                invocation,
                KnownSymbol.DependencyProperty.OverrideMetadata,
                semanticModel,
                cancellationToken,
                out method);
        }

        internal static bool IsPotentialDependencyPropertyBackingField(BackingFieldOrProperty fieldOrProperty)
        {
            return fieldOrProperty.Type == KnownSymbol.DependencyProperty;
        }

        internal static bool IsPotentialDependencyPropertyKeyBackingField(BackingFieldOrProperty fieldOrProperty)
        {
            return fieldOrProperty.Type == KnownSymbol.DependencyPropertyKey;
        }

        internal static bool TryGetRegisteredName(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out string registeredName)
        {
            registeredName = null;
            if (invocation == null)
            {
                return false;
            }

            if (TryGetRegisterCall(invocation, semanticModel, cancellationToken, out _) ||
                TryGetRegisterReadOnlyCall(invocation, semanticModel, cancellationToken, out _) ||
                TryGetRegisterAttachedCall(invocation, semanticModel, cancellationToken, out _) ||
                TryGetRegisterAttachedReadOnlyCall(invocation, semanticModel, cancellationToken, out _))
            {
                var nameArg = invocation.ArgumentList?.Arguments.FirstOrDefault();
                return nameArg?.TryGetStringValue(semanticModel, cancellationToken, out registeredName) == true;
            }

            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                (TryGetAddOwnerCall(invocation, semanticModel, cancellationToken, out _) ||
                 TryGetOverrideMetadataCall(invocation, semanticModel, cancellationToken, out _)))
            {
                if (BackingFieldOrProperty.TryCreate(semanticModel.GetSymbolSafe(memberAccess.Expression, cancellationToken), out var fieldOrProperty))
                {
                    return TryGetRegisteredName(fieldOrProperty, semanticModel, cancellationToken, out registeredName);
                }

                return false;
            }

            return false;
        }

        internal static bool TryGetRegisteredName(BackingFieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken, out string result)
        {
            result = null;
            if (TryGetRegisterInvocationRecursive(fieldOrProperty, semanticModel, cancellationToken, out var invocation))
            {
                if (invocation.TryGetArgumentAtIndex(0, out var arg))
                {
                    return arg.TryGetStringValue(semanticModel, cancellationToken, out result);
                }

                return false;
            }

            if (TryGetPropertyByName(fieldOrProperty, out var property))
            {
                result = property.Name;
                return true;
            }

            return false;
        }

        internal static bool TryGetRegisteredType(BackingFieldOrProperty field, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol result)
        {
            result = null;
            if (TryGetRegisterInvocationRecursive(field, semanticModel, cancellationToken, out var invocation))
            {
                if (invocation.TryGetArgumentAtIndex(1, out var typeArg))
                {
                    if (!typeArg.TryGetTypeofValue(semanticModel, cancellationToken, out result))
                    {
                        return false;
                    }

                    if (result.Kind == SymbolKind.TypeParameter)
                    {
                        var index = field.ContainingType.TypeParameters.IndexOf((ITypeParameterSymbol)result);
                        if (index < 0)
                        {
                            result = null;
                            return false;
                        }

                        result = field.ContainingType.TypeArguments[index];
                    }

                    return result != null;
                }

                return false;
            }

            if (TryGetPropertyByName(field, out var property))
            {
                result = property.Type;
                return true;
            }

            return false;
        }

        internal static bool TryGetDependencyPropertyKeyField(BackingFieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty result)
        {
            result = default(BackingFieldOrProperty);
            if (fieldOrProperty.TryGetAssignedValue(cancellationToken, out var value))
            {
                var symbol = semanticModel.GetSymbolSafe(value, cancellationToken);
                if (symbol is IMethodSymbol method)
                {
                    if (method != KnownSymbol.DependencyProperty.AddOwner)
                    {
                        return false;
                    }

                    var invocation = (InvocationExpressionSyntax)value;
                    var member = invocation.Expression as MemberAccessExpressionSyntax;

                    return BackingFieldOrProperty.TryCreate(semanticModel.GetSymbolSafe(member?.Expression, cancellationToken), out result) &&
                           TryGetDependencyPropertyKeyField(result, semanticModel, cancellationToken, out result);
                }

                if (symbol is IPropertySymbol property &&
                    property == KnownSymbol.DependencyPropertyKey.DependencyProperty &&
                    value is MemberAccessExpressionSyntax memberAccess)
                {
                    return BackingFieldOrProperty.TryCreate(semanticModel.GetSymbolSafe(memberAccess.Expression, cancellationToken), out result);
                }
            }

            return false;
        }

        internal static bool TryGetDependencyAddOwnerSourceField(BackingFieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty result)
        {
            result = default(BackingFieldOrProperty);
            if (fieldOrProperty.TryGetAssignedValue(cancellationToken, out var value) &&
                value is InvocationExpressionSyntax invocation)
            {
                var invocationSymbol = semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol;
                if (invocationSymbol == KnownSymbol.DependencyProperty.AddOwner)
                {
                    var addOwner = (MemberAccessExpressionSyntax)invocation.Expression;
                    return BackingFieldOrProperty.TryCreate(
                        semanticModel.GetSymbolSafe(addOwner.Expression, cancellationToken),
                        out result);
                }
            }

            return false;
        }

        internal static bool TryGetRegisterInvocation(BackingFieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken, out InvocationExpressionSyntax result)
        {
            result = null;
            if (fieldOrProperty.TryGetAssignedValue(cancellationToken, out var value) &&
                value is InvocationExpressionSyntax invocation)
            {
                if (TryGetRegisterCall(invocation, semanticModel, cancellationToken, out _) ||
                    TryGetRegisterReadOnlyCall(invocation, semanticModel, cancellationToken, out _) ||
                    TryGetRegisterAttachedCall(invocation, semanticModel, cancellationToken, out _) ||
                    TryGetRegisterAttachedReadOnlyCall(invocation, semanticModel, cancellationToken, out _))
                {
                    result = invocation;
                    return true;
                }
            }

            return false;
        }

        internal static bool TryGetRegisterInvocationRecursive(BackingFieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken, out InvocationExpressionSyntax result)
        {
            if (TryGetDependencyPropertyKeyField(fieldOrProperty, semanticModel, cancellationToken, out var keyField))
            {
                return TryGetRegisterInvocationRecursive(keyField, semanticModel, cancellationToken, out result);
            }

            if (TryGetDependencyAddOwnerSourceField(fieldOrProperty, semanticModel, cancellationToken, out var addOwnerSource))
            {
                return TryGetRegisterInvocationRecursive(addOwnerSource, semanticModel, cancellationToken, out result);
            }

            return TryGetRegisterInvocation(fieldOrProperty, semanticModel, cancellationToken, out result);
        }

        internal static bool TryGetPropertyByName(BackingFieldOrProperty fieldOrProperty, out IPropertySymbol property)
        {
            property = null;

            if (IsPotentialDependencyPropertyBackingField(fieldOrProperty) ||
                IsPotentialDependencyPropertyKeyBackingField(fieldOrProperty))
            {
                var suffix = IsPotentialDependencyPropertyBackingField(fieldOrProperty)
                                 ? "Property"
                                 : "PropertyKey";

                foreach (var symbol in fieldOrProperty.ContainingType.GetMembers())
                {
                    var candidate = symbol as IPropertySymbol;
                    if (candidate == null)
                    {
                        continue;
                    }

                    if (!fieldOrProperty.Name.IsParts(candidate.Name, suffix))
                    {
                        continue;
                    }

                    if (property != null)
                    {
                        property = null;
                        return false;
                    }

                    property = symbol as IPropertySymbol;
                }
            }

            return property != null;
        }

        /// <summary>
        /// This is an optimization to avoid calling <see cref="SemanticModel.GetSymbolInfo"/>
        /// </summary>
        private static bool TryGetCall(InvocationExpressionSyntax invocation, QualifiedMethod qualifiedMethod, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            method = null;
            if (invocation.TryGetInvokedMethodName(out var name) &&
                name != qualifiedMethod.Name)
            {
                return false;
            }

            method = semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol;
            return method == qualifiedMethod;
        }
    }
}