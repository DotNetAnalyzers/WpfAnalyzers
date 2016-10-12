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
        internal static bool TryGetSingleBackingField(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol result)
        {
            result = null;
            IFieldSymbol getter;
            IFieldSymbol setter;
            PropertyDeclarationSyntax propertyDeclaration;
            if (TryGetPropertyDeclaration(property, cancellationToken, out propertyDeclaration))
            {
                if (TryGetBackingFields(
                    property,
                    semanticModel,
                    cancellationToken,
                    out getter,
                    out setter))
                {
                    if (ReferenceEquals(setter, getter) && setter.Type.Name == Names.DependencyProperty)
                    {
                        result = setter;
                        return true;
                    }
                }
            }

            if (TryGetBackingFieldsUsingReflection(property, out getter, out setter))
            {
                if (ReferenceEquals(getter, setter))
                {
                    result = getter;
                    return result?.Type.Name == Names.DependencyProperty;
                }
            }

            return false;
        }

        internal static bool TryGetRegisteredName(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out string result)
        {
            result = null;
            PropertyDeclarationSyntax propertyDeclaration;
            if (TryGetPropertyDeclaration(property, cancellationToken, out propertyDeclaration))
            {
                return TryGetRegisteredName(
                    propertyDeclaration,
                    semanticModel,
                    cancellationToken,
                    out result);
            }

            return false;
        }

        internal static bool TryGetRegisteredName(PropertyDeclarationSyntax property, SemanticModel semanticModel, CancellationToken cancellationToken, out string result)
        {
            IFieldSymbol field;
            if (TryGetRegisterField(property, semanticModel, cancellationToken, out field))
            {
                return DependencyProperty.TryGetRegisteredName(field, semanticModel, cancellationToken, out result);
            }

            result = null;
            return false;
        }

        internal static bool TryGetRegisteredType(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol result)
        {
            result = null;
            PropertyDeclarationSyntax propertyDeclaration;
            if (TryGetPropertyDeclaration(property, cancellationToken, out propertyDeclaration))
            {
                return TryGetRegisteredType(
                    propertyDeclaration,
                    semanticModel,
                    cancellationToken,
                    out result);
            }

            return false;
        }

        internal static bool TryGetRegisteredType(PropertyDeclarationSyntax property, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol result)
        {
            IFieldSymbol field;
            if (TryGetRegisterField(property, semanticModel, cancellationToken, out field))
            {
                return DependencyProperty.TryGetRegisteredType(field, semanticModel, cancellationToken, out result);
            }

            result = null;
            return false;
        }

        internal static bool TryGetBackingFields(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol getField, out IFieldSymbol setField)
        {
            getField = null;
            setField = null;

            PropertyDeclarationSyntax propertyDeclaration;
            if (TryGetPropertyDeclaration(property, cancellationToken, out propertyDeclaration))
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

        internal static bool TryGetBackingFields(PropertyDeclarationSyntax propertyDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol getField, out IFieldSymbol setField)
        {
            getField = null;
            setField = null;
            AccessorDeclarationSyntax getAccessor;
            AccessorDeclarationSyntax setAccessor;
            if (propertyDeclaration.TryGetAccessorDeclaration(SyntaxKind.GetAccessorDeclaration, out getAccessor) &&
                propertyDeclaration.TryGetAccessorDeclaration(SyntaxKind.SetAccessorDeclaration, out setAccessor))
            {
                using (var getterWalker = ClrGetterWalker.Create(semanticModel, cancellationToken, getAccessor))
                {
                    using (var setterWalker = ClrSetterWalker.Create(semanticModel, cancellationToken, setAccessor))
                    {
                        if (getterWalker.HasError ||
                            setterWalker.HasError)
                        {
                            return false;
                        }

                        if (getterWalker.IsSuccess &&
                            getterWalker.Property.TryGetSymbol(semanticModel, cancellationToken, out getField) &&
                            setterWalker.IsSuccess &&
                            setterWalker.Property.TryGetSymbol(semanticModel, cancellationToken, out setField))
                        {
                            return true;
                        }

                        var property = semanticModel.GetSymbolInfo(propertyDeclaration, cancellationToken)
                                                    .Symbol as IPropertySymbol;
                        return TryGetBackingFieldsUsingReflection(property, out getField, out setField);
                    }
                }
            }

            return false;
        }

        internal static bool TryGetBackingFieldsUsingReflection(IPropertySymbol property, out IFieldSymbol getter, out IFieldSymbol setter)
        {
            getter = null;
            setter = null;
            if (property?.ContainingType.IsAssignableToDependencyObject() != true)
            {
                return false;
            }

            foreach (var field in property.ContainingType.GetMembers().OfType<IFieldSymbol>())
            {
                if (field.Name.IsParts(property.Name, "Property"))
                {
                    if (!DependencyProperty.IsPotentialBackingField(field))
                    {
                        getter = null;
                        setter = null;
                        return false;
                    }

                    getter = field;
                }

                if (field.Name.IsParts(property.Name, "PropertyKey"))
                {
                    if (!DependencyProperty.IsPotentialBackingKeyField(field))
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

            SyntaxReference reference;
            if (property.DeclaringSyntaxReferences.TryGetLast(out reference))
            {
                result = reference.GetSyntax(cancellationToken) as PropertyDeclarationSyntax;
                return result != null;
            }

            return false;
        }

        private static bool TryGetRegisterField(PropertyDeclarationSyntax property, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol result)
        {
            result = null;
            IFieldSymbol getter;
            IFieldSymbol setter;
            if (TryGetBackingFields(
                property,
                semanticModel,
                cancellationToken,
                out getter,
                out setter))
            {
                IFieldSymbol keyField;
                if (DependencyProperty.TryGetDependencyPropertyKeyField(
                    getter,
                    semanticModel,
                    cancellationToken,
                    out keyField))
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