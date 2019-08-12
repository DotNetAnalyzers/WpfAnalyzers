namespace WpfAnalyzers
{
    using System.Linq;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class DependencyProperty
    {
        internal static bool TryGetRegisterCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return semanticModel.TryGetSymbol(invocation, KnownSymbol.DependencyProperty.Register, cancellationToken, out method);
        }

        internal static bool TryGetRegisterReadOnlyCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return semanticModel.TryGetSymbol(invocation, KnownSymbol.DependencyProperty.RegisterReadOnly, cancellationToken, out method);
        }

        internal static bool TryGetRegisterAttachedCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return semanticModel.TryGetSymbol(invocation, KnownSymbol.DependencyProperty.RegisterAttached, cancellationToken, out method);
        }

        internal static bool TryGetRegisterAttachedReadOnlyCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return semanticModel.TryGetSymbol(invocation, KnownSymbol.DependencyProperty.RegisterAttachedReadOnly, cancellationToken, out method);
        }

        internal static bool TryGetAddOwnerCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return semanticModel.TryGetSymbol(invocation, KnownSymbol.DependencyProperty.AddOwner, cancellationToken, out method);
        }

        internal static bool TryGetOverrideMetadataCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return semanticModel.TryGetSymbol(invocation, KnownSymbol.DependencyProperty.OverrideMetadata, cancellationToken, out method);
        }

        internal static bool IsPotentialDependencyPropertyBackingField(BackingFieldOrProperty fieldOrProperty)
        {
            return fieldOrProperty.Type == KnownSymbol.DependencyProperty;
        }

        internal static bool IsPotentialDependencyPropertyKeyBackingField(BackingFieldOrProperty fieldOrProperty)
        {
            return fieldOrProperty.Type == KnownSymbol.DependencyPropertyKey;
        }

        internal static bool TryGetRegisteredName(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax nameArg, out string registeredName)
        {
            nameArg = null;
            registeredName = null;
            if (invocation == null)
            {
                return false;
            }

            if (TryGetRegisterCall(invocation, semanticModel, cancellationToken, out var method) ||
                TryGetRegisterReadOnlyCall(invocation, semanticModel, cancellationToken, out method) ||
                TryGetRegisterAttachedCall(invocation, semanticModel, cancellationToken, out method) ||
                TryGetRegisterAttachedReadOnlyCall(invocation, semanticModel, cancellationToken, out method))
            {
                return method.TryFindParameter("name", out var parameter) &&
                       invocation.TryFindArgument(parameter, out nameArg) &&
                       nameArg.TryGetStringValue(semanticModel, cancellationToken, out registeredName);
            }

            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                (TryGetAddOwnerCall(invocation, semanticModel, cancellationToken, out _) ||
                 TryGetOverrideMetadataCall(invocation, semanticModel, cancellationToken, out _)))
            {
                if (semanticModel.TryGetSymbol(memberAccess.Expression, cancellationToken, out var symbol) &&
                    BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out var fieldOrProperty))
                {
                    return TryGetRegisteredName(fieldOrProperty, semanticModel, cancellationToken, out nameArg, out registeredName);
                }

                return false;
            }

            return false;
        }

        internal static bool TryGetRegisteredName(BackingFieldOrProperty backing, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax nameArg, out string result)
        {
            nameArg = null;
            result = null;
            if (TryGetRegisterInvocationRecursive(backing, semanticModel, cancellationToken, out var invocation, out var method))
            {
                return method.TryFindParameter("name", out var parameter) &&
                       invocation.TryFindArgument(parameter, out nameArg) &&
                       nameArg.TryGetStringValue(semanticModel, cancellationToken, out result);
            }

            if (TryGetDependencyAddOwnerSourceField(backing, semanticModel, cancellationToken, out var source) &&
                !source.Symbol.Equals(backing.Symbol))
            {
                return TryGetRegisteredName(source, semanticModel, cancellationToken, out nameArg, out result);
            }

            if (backing.Symbol.Locations.All(x => !x.IsInSource) &&
                TryGetPropertyByName(backing, out var property))
            {
                result = property.Name;
                return true;
            }

            return false;
        }

        internal static bool TryGetRegisteredType(BackingFieldOrProperty backing, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol result)
        {
            result = null;
            if (TryGetRegisterInvocationRecursive(backing, semanticModel, cancellationToken, out var invocation, out _))
            {
                return invocation.TryGetArgumentAtIndex(1, out var typeArg) &&
                       typeArg.Expression is TypeOfExpressionSyntax typeOf &&
                       TypeOf.TryGetType(typeOf, backing.ContainingType, semanticModel, cancellationToken, out result);
            }

            if (TryGetDependencyAddOwnerSourceField(backing, semanticModel, cancellationToken, out var source) &&
               !source.Symbol.Equals(backing.Symbol))
            {
                return TryGetRegisteredType(source, semanticModel, cancellationToken, out result);
            }

            if (backing.Symbol.Locations.All(x => !x.IsInSource) &&
                TryGetPropertyByName(backing, out var property))
            {
                result = property.Type;
                return true;
            }

            return false;
        }

        internal static bool TryGetDependencyPropertyKeyFieldOrProperty(BackingFieldOrProperty backing, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty result)
        {
            result = default;
            if (backing.TryGetAssignedValue(cancellationToken, out var value) &&
                semanticModel.TryGetSymbol(value, cancellationToken, out ISymbol symbol))
            {
                if (symbol is IMethodSymbol method)
                {
                    return method == KnownSymbol.DependencyProperty.AddOwner &&
                           value is InvocationExpressionSyntax invocation &&
                           invocation.Expression is MemberAccessExpressionSyntax member &&
                           semanticModel.TryGetSymbol(member.Expression, cancellationToken, out ISymbol candidate) &&
                           BackingFieldOrProperty.TryCreateForDependencyProperty(candidate, out result) &&
                           TryGetDependencyPropertyKeyFieldOrProperty(result, semanticModel, cancellationToken, out result);
                }
                else
                {
                    return symbol is IPropertySymbol property &&
                           property == KnownSymbol.DependencyPropertyKey.DependencyProperty &&
                           value is MemberAccessExpressionSyntax memberAccess &&
                           semanticModel.TryGetSymbol(memberAccess.Expression, cancellationToken, out ISymbol candidate) &&
                           BackingFieldOrProperty.TryCreateForDependencyProperty(candidate, out result);
                }
            }

            return false;
        }

        internal static bool TryGetDependencyAddOwnerSourceField(BackingFieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty result)
        {
            result = default;
            if (fieldOrProperty.TryGetAssignedValue(cancellationToken, out var value) &&
                value is InvocationExpressionSyntax invocation &&
                semanticModel.TryGetSymbol(invocation, KnownSymbol.DependencyProperty.AddOwner, cancellationToken, out _))
            {
                var addOwner = (MemberAccessExpressionSyntax)invocation.Expression;
                return BackingFieldOrProperty.TryCreateForDependencyProperty(
                    semanticModel.GetSymbolSafe(addOwner.Expression, cancellationToken),
                    out result);
            }

            return false;
        }

        internal static bool TryGetRegisterInvocation(BackingFieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken, out InvocationExpressionSyntax result, out IMethodSymbol symbol)
        {
            symbol = null;
            result = null;
            if (fieldOrProperty.TryGetAssignedValue(cancellationToken, out var value) &&
                value is InvocationExpressionSyntax invocation)
            {
                if (TryGetRegisterCall(invocation, semanticModel, cancellationToken, out symbol) ||
                    TryGetRegisterReadOnlyCall(invocation, semanticModel, cancellationToken, out symbol) ||
                    TryGetRegisterAttachedCall(invocation, semanticModel, cancellationToken, out symbol) ||
                    TryGetRegisterAttachedReadOnlyCall(invocation, semanticModel, cancellationToken, out symbol))
                {
                    result = invocation;
                    return true;
                }
            }

            return false;
        }

        internal static bool TryGetRegisterInvocationRecursive(BackingFieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken, out InvocationExpressionSyntax result, out IMethodSymbol symbol)
        {
            if (TryGetDependencyPropertyKeyFieldOrProperty(fieldOrProperty, semanticModel, cancellationToken, out var keyField))
            {
                return TryGetRegisterInvocationRecursive(keyField, semanticModel, cancellationToken, out result, out symbol);
            }

            if (TryGetDependencyAddOwnerSourceField(fieldOrProperty, semanticModel, cancellationToken, out var addOwnerSource))
            {
                return TryGetRegisterInvocationRecursive(addOwnerSource, semanticModel, cancellationToken, out result, out symbol);
            }

            return TryGetRegisterInvocation(fieldOrProperty, semanticModel, cancellationToken, out result, out symbol);
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
                    if (symbol is IPropertySymbol candidate)
                    {
                        if (!fieldOrProperty.Name.IsParts(candidate.Name, suffix))
                        {
                            continue;
                        }

                        if (property != null)
                        {
                            property = null;
                            return false;
                        }

                        property = candidate;
                    }
                }
            }

            return property != null;
        }
    }
}
