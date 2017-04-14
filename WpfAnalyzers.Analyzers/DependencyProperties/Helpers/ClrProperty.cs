namespace WpfAnalyzers.DependencyProperties
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Exposes helper methods for working with CLR-properties for DependencyProperties
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

            if (TryGetPropertyDeclaration(property, cancellationToken, out PropertyDeclarationSyntax propertyDeclaration))
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
            return TryGetBackingFields(property, semanticModel, cancellationToken, out IFieldSymbol getField, out IFieldSymbol setField);
        }

        /// <summary>
        /// Get the single DependencyProperty backing field for <paramref name="property"/>
        /// Returns false for accessors for readonly dependency properties.
        /// </summary>
        internal static bool TryGetSingleBackingField(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol result)
        {
            result = null;
            IFieldSymbol getter;
            IFieldSymbol setter;
            if (TryGetPropertyDeclaration(property, cancellationToken, out PropertyDeclarationSyntax propertyDeclaration))
            {
                if (TryGetBackingFields(
                    property,
                    semanticModel,
                    cancellationToken,
                    out getter,
                    out setter))
                {
                    if (ReferenceEquals(setter, getter) &&
                        setter.Type == KnownSymbol.DependencyProperty)
                    {
                        result = setter;
                        return true;
                    }
                }
            }

            if (TryGetBackingFieldsByName(property, out getter, out setter))
            {
                if (ReferenceEquals(getter, setter))
                {
                    result = getter;
                    return result?.Type == KnownSymbol.DependencyProperty;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the name the backing DependencyProperty is registered with.
        /// </summary>
        internal static bool TryGetRegisteredName(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out string result)
        {
            result = null;
            if (TryGetPropertyDeclaration(property, cancellationToken, out PropertyDeclarationSyntax propertyDeclaration))
            {
                return TryGetRegisteredName(
                    propertyDeclaration,
                    semanticModel,
                    cancellationToken,
                    out result);
            }

            return false;
        }

        /// <summary>
        /// Get the name the backing DependencyProperty is registered with.
        /// </summary>
        internal static bool TryGetRegisteredName(PropertyDeclarationSyntax property, SemanticModel semanticModel, CancellationToken cancellationToken, out string result)
        {
            if (TryGetRegisterField(property, semanticModel, cancellationToken, out IFieldSymbol field))
            {
                return DependencyProperty.TryGetRegisteredName(field, semanticModel, cancellationToken, out result);
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Get the value type the backing DependencyProperty is registered with.
        /// </summary>
        internal static bool TryGetRegisteredType(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol result)
        {
            result = null;
            if (TryGetPropertyDeclaration(property, cancellationToken, out PropertyDeclarationSyntax propertyDeclaration))
            {
                return TryGetRegisteredType(
                    propertyDeclaration,
                    semanticModel,
                    cancellationToken,
                    out result);
            }

            return false;
        }

        /// <summary>
        /// Get the value type the backing DependencyProperty is registered with.
        /// </summary>
        internal static bool TryGetRegisteredType(PropertyDeclarationSyntax property, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol result)
        {
            if (TryGetRegisterField(property, semanticModel, cancellationToken, out IFieldSymbol field))
            {
                return DependencyProperty.TryGetRegisteredType(field, semanticModel, cancellationToken, out result);
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Get the backing fields for the <paramref name="property"/> these are different for readonly dependency properties where the setter returns the DependencyPropertyKey field
        /// </summary>
        internal static bool TryGetBackingFields(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol getField, out IFieldSymbol setField)
        {
            getField = null;
            setField = null;

            if (TryGetPropertyDeclaration(property, cancellationToken, out PropertyDeclarationSyntax propertyDeclaration))
            {
                if (TryGetBackingFields(propertyDeclaration, semanticModel, cancellationToken, out getField, out setField))
                {
                    if (getField.ContainingType.IsGenericType)
                    {
                        return property.ContainingType.TryGetField(getField.Name, out getField) &&
                               property.ContainingType.TryGetField(setField.Name, out setField);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the backing fields for the <paramref name="propertyDeclaration"/> these are different for readonly dependency properties where the setter returns the DependencyPropertyKey field
        /// </summary>
        internal static bool TryGetBackingFields(PropertyDeclarationSyntax propertyDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol getField, out IFieldSymbol setField)
        {
            getField = null;
            setField = null;
            if (propertyDeclaration.TryGetAccessorDeclaration(SyntaxKind.GetAccessorDeclaration, out AccessorDeclarationSyntax getAccessor) &&
propertyDeclaration.TryGetAccessorDeclaration(SyntaxKind.SetAccessorDeclaration, out AccessorDeclarationSyntax setAccessor))
            {
                using (var pooled = ClrGetterWalker.Create(semanticModel, cancellationToken, getAccessor))
                {
                    using (var setterWalker = ClrSetterWalker.Create(semanticModel, cancellationToken, setAccessor))
                    {
                        if (pooled.Item.HasError ||
                            setterWalker.Item.HasError)
                        {
                            return false;
                        }

                        if (pooled.Item.IsSuccess &&
                            pooled.Item.Property.TryGetSymbol(semanticModel, cancellationToken, out getField) &&
                            setterWalker.Item.IsSuccess &&
                            setterWalker.Item.Property.TryGetSymbol(semanticModel, cancellationToken, out setField))
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

        /// <summary>
        /// Get the backing fields for the <paramref name="property"/> these are different for readonly dependency properties where the setter returns the DependencyPropertyKey field.
        /// This method looks for fields that matches the name NameProperty and NamePropertyKey.
        /// </summary>
        internal static bool TryGetBackingFieldsByName(IPropertySymbol property, out IFieldSymbol getter, out IFieldSymbol setter)
        {
            getter = null;
            setter = null;
            if (property == null ||
                !property.ContainingType.Is(KnownSymbol.DependencyObject))
            {
                return false;
            }

            foreach (var field in property.ContainingType.GetMembers().OfType<IFieldSymbol>())
            {
                if (field.Name.IsParts(property.Name, "Property"))
                {
                    if (!DependencyProperty.IsPotentialDependencyPropertyBackingField(field))
                    {
                        getter = null;
                        setter = null;
                        return false;
                    }

                    getter = field;
                }

                if (field.Name.IsParts(property.Name, "PropertyKey"))
                {
                    if (!DependencyProperty.IsPotentialDependencyPropertyKeyBackingField(field))
                    {
                        getter = null;
                        setter = null;
                        return false;
                    }

                    setter = field;
                }
            }

            if (setter == null)
            {
                setter = getter;
            }

            return setter != null;
        }

        internal static bool TryGetPropertyDeclaration(IPropertySymbol property, CancellationToken cancellationToken, out PropertyDeclarationSyntax result)
        {
            result = null;
            if (!property.IsPotentialClrProperty())
            {
                return false;
            }

            if (property.DeclaringSyntaxReferences.TryGetLast(out SyntaxReference reference))
            {
                result = reference.GetSyntax(cancellationToken) as PropertyDeclarationSyntax;
                return result != null;
            }

            return false;
        }

        private static bool TryGetRegisterField(PropertyDeclarationSyntax property, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol result)
        {
            result = null;
            if (TryGetBackingFields(
property,
semanticModel,
cancellationToken,
out IFieldSymbol getter,
out IFieldSymbol setter))
            {
                if (DependencyProperty.TryGetDependencyPropertyKeyField(
    getter,
    semanticModel,
    cancellationToken,
    out IFieldSymbol keyField))
                {
                    getter = keyField;
                }

                if (ReferenceEquals(setter, getter))
                {
                    result = setter;
                    return true;
                }
            }

            return false;
        }
    }
}