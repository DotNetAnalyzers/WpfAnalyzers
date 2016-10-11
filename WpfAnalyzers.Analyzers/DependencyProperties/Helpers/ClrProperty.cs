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
        internal static bool TryGetRegisteredName(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out string result)
        {
            result = null;
            PropertyDeclarationSyntax propertyDeclaration;
            if (TryGetPropertyDeclaration(property, semanticModel, cancellationToken, out propertyDeclaration))
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
            if (TryGetPropertyDeclaration(property, semanticModel, cancellationToken, out propertyDeclaration))
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

        internal static bool TryGetBackingFields(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol getter, out IFieldSymbol setter)
        {
            getter = null;
            setter = null;

            PropertyDeclarationSyntax propertyDeclaration;
            if (TryGetPropertyDeclaration(property, semanticModel, cancellationToken, out propertyDeclaration))
            {
                return TryGetBackingFields(
                    propertyDeclaration,
                    semanticModel,
                    cancellationToken,
                    out getter,
                    out setter);
            }

            return false;
        }

        internal static bool TryGetBackingFields(PropertyDeclarationSyntax property, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol getter, out IFieldSymbol setter)
        {
            getter = null;
            setter = null;
            AccessorDeclarationSyntax getAccessor;
            AccessorDeclarationSyntax setAccessor;
            if (property.TryGetAccessorDeclaration(SyntaxKind.GetAccessorDeclaration, out getAccessor) &&
                property.TryGetAccessorDeclaration(SyntaxKind.SetAccessorDeclaration, out setAccessor))
            {
                using (var getterWalker = ClrGetterWalker.Create(semanticModel, cancellationToken, getAccessor))
                {
                    using (var setterWalker = ClrSetterWalker.Create(semanticModel, cancellationToken, setAccessor))
                    {
                        if (getterWalker.IsSuccess &&
                            setterWalker.IsSuccess)
                        {
                            getter = semanticModel.GetSymbolInfo(getterWalker.Property.Expression, cancellationToken).Symbol as IFieldSymbol;
                            setter = semanticModel.GetSymbolInfo(setterWalker.Property.Expression, cancellationToken).Symbol as IFieldSymbol;

                            return getter != null && setter != null;
                        }
                    }
                }
            }

            return false;
        }

        internal static bool TryGetPropertyDeclaration(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out PropertyDeclarationSyntax result)
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