namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Exposes helper methods for working with CLR-properties for DependencyProperty
    /// </summary>
    internal static class ClrProperty
    {
        /// <summary>
        /// Check if the <paramref name="property"/> can be an accessor for a DependencyProperty
        /// </summary>
        internal static bool IsPotentialClrProperty(this IPropertySymbol property)
        {
            return property != null &&
                   !property.IsIndexer &&
                   !property.IsReadOnly &&
                   !property.IsWriteOnly &&
                   !property.IsStatic &&
                   property.ContainingType.Is(KnownSymbol.DependencyObject);
        }

        /// <summary>
        /// Check if the <paramref name="property"/> is a CLR accessor for a DependencyProperty
        /// </summary>
        internal static bool IsDependencyPropertyAccessor(this IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (!property.IsPotentialClrProperty())
            {
                return false;
            }

            if (TryGetPropertyDeclaration(property, cancellationToken, out var propertyDeclaration))
            {
                return IsDependencyPropertyAccessor(propertyDeclaration, semanticModel, cancellationToken);
            }

            return false;
        }

        /// <summary>
        /// Check if the <paramref name="property"/> is a CLR accessor for a DependencyProperty
        /// </summary>
        internal static bool IsDependencyPropertyAccessor(PropertyDeclarationSyntax property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return TryGetBackingFields(property, semanticModel, cancellationToken, out var _, out var _);
        }

        /// <summary>
        /// Get the single DependencyProperty backing field for <paramref name="property"/>
        /// Returns false for accessors for readonly dependency properties.
        /// </summary>
        internal static bool TryGetSingleBackingField(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty result)
        {
            result = default(BackingFieldOrProperty);
            BackingFieldOrProperty getter;
            BackingFieldOrProperty setter;
            if (TryGetPropertyDeclaration(property, cancellationToken, out _))
            {
                if (TryGetBackingFields(
                    property,
                    semanticModel,
                    cancellationToken,
                    out getter,
                    out setter))
                {
                    if (ReferenceEquals(setter.Symbol, getter.Symbol) &&
                        setter.Type == KnownSymbol.DependencyProperty)
                    {
                        result = setter;
                        return true;
                    }
                }
            }

            if (TryGetBackingFieldsByName(property, out getter, out setter))
            {
                if (ReferenceEquals(getter.Symbol, setter.Symbol))
                {
                    result = getter;
                    return result.Type == KnownSymbol.DependencyProperty;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the backing fields for the <paramref name="propertyDeclaration"/> these are different for readonly dependency properties where the setter returns the DependencyPropertyKey field
        /// </summary>
        internal static bool TryGetBackingFields(PropertyDeclarationSyntax propertyDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty getField, out BackingFieldOrProperty setField)
        {
            getField = default(BackingFieldOrProperty);
            setField = default(BackingFieldOrProperty);
            if (propertyDeclaration.TryGetAccessorDeclaration(SyntaxKind.GetAccessorDeclaration, out var getAccessor) &&
                propertyDeclaration.TryGetAccessorDeclaration(SyntaxKind.SetAccessorDeclaration, out var setAccessor))
            {
                using (var getterWalker = ClrGetterWalker.Borrow(semanticModel, cancellationToken, getAccessor))
                {
                    using (var setterWalker = ClrSetterWalker.Borrow(semanticModel, cancellationToken, setAccessor))
                    {
                        if (getterWalker.HasError ||
                            setterWalker.HasError)
                        {
                            return false;
                        }

                        if (getterWalker.IsSuccess &&
                            BackingFieldOrProperty.TryCreate(semanticModel.GetSymbolSafe(getterWalker.Property.Expression, cancellationToken), out getField) &&
                            setterWalker.IsSuccess &&
                            BackingFieldOrProperty.TryCreate(semanticModel.GetSymbolSafe(setterWalker.Property.Expression, cancellationToken), out setField))
                        {
                            return true;
                        }

                        var property = semanticModel.GetSymbolSafe(propertyDeclaration, cancellationToken) as IPropertySymbol;
                        return TryGetBackingFieldsByName(property, out getField, out setField);
                    }
                }
            }

            return false;
        }

        internal static bool TryGetRegisterField(PropertyDeclarationSyntax property, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty result)
        {
            result = default(BackingFieldOrProperty);
            if (TryGetBackingFields(property, semanticModel, cancellationToken, out var getter, out var setter))
            {
                if (DependencyProperty.TryGetDependencyPropertyKeyField(getter, semanticModel, cancellationToken, out var keyField))
                {
                    getter = keyField;
                }

                if (ReferenceEquals(setter.Symbol, getter.Symbol))
                {
                    result = setter;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the backing fields for the <paramref name="property"/> these are different for readonly dependency properties where the setter returns the DependencyPropertyKey field
        /// </summary>
        private static bool TryGetBackingFields(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty getField, out BackingFieldOrProperty setField)
        {
            getField = default(BackingFieldOrProperty);
            setField = default(BackingFieldOrProperty);

            if (TryGetPropertyDeclaration(property, cancellationToken, out var propertyDeclaration))
            {
                if (TryGetBackingFields(propertyDeclaration, semanticModel, cancellationToken, out getField, out setField))
                {
                    if (getField.ContainingType.IsGenericType)
                    {
                        return property.ContainingType.TryGetSingleMemberRecursive<ISymbol>(getField.Name, out var getMember) &&
                               BackingFieldOrProperty.TryCreate(getMember, out getField) &&
                               property.ContainingType.TryGetSingleMemberRecursive<ISymbol>(setField.Name, out var setMember) &&
                               BackingFieldOrProperty.TryCreate(setMember, out setField);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the backing fields for the <paramref name="property"/> these are different for readonly dependency properties where the setter returns the DependencyPropertyKey field.
        /// This method looks for fields that matches the name NameProperty and NamePropertyKey.
        /// </summary>
        private static bool TryGetBackingFieldsByName(IPropertySymbol property, out BackingFieldOrProperty getter, out BackingFieldOrProperty setter)
        {
            getter = default(BackingFieldOrProperty);
            setter = default(BackingFieldOrProperty);
            if (property == null ||
                !property.ContainingType.Is(KnownSymbol.DependencyObject))
            {
                return false;
            }

            foreach (var member in property.ContainingType.GetMembers())
            {
                if (BackingFieldOrProperty.TryCreate(member, out var candidate))
                {
                    if (candidate.Name.IsParts(property.Name, "Property"))
                    {
                        if (!DependencyProperty.IsPotentialDependencyPropertyBackingField(candidate))
                        {
                            return false;
                        }

                        getter = candidate;
                    }

                    if (candidate.Name.IsParts(property.Name, "PropertyKey"))
                    {
                        if (!DependencyProperty.IsPotentialDependencyPropertyKeyBackingField(candidate))
                        {
                            return false;
                        }

                        setter = candidate;
                    }
                }
            }

            if (setter.Symbol == null)
            {
                setter = getter;
            }

            return setter.Symbol != null;
        }

        private static bool TryGetPropertyDeclaration(IPropertySymbol property, CancellationToken cancellationToken, out PropertyDeclarationSyntax result)
        {
            result = null;
            if (!property.IsPotentialClrProperty())
            {
                return false;
            }

            if (property.DeclaringSyntaxReferences.TryGetLast(out var reference))
            {
                result = reference.GetSyntax(cancellationToken) as PropertyDeclarationSyntax;
                return result != null;
            }

            return false;
        }
    }
}