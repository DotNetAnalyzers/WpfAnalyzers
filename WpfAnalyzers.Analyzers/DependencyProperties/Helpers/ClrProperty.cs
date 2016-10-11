namespace WpfAnalyzers.DependencyProperties
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ClrProperty
    {
        internal static bool TryGetRegisteredName(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out string result)
        {
            result = null;
            if (!property.IsPotentialClrProperty())
            {
                return false;
            }

            SyntaxReference reference;
            if (property.DeclaringSyntaxReferences.TryGetLast(out reference))
            {
                var propertyDeclaration = reference.GetSyntax(cancellationToken) as PropertyDeclarationSyntax;
                if (propertyDeclaration == null)
                {
                    return false;
                }

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
                    return DependencyProperty.TryGetRegisteredName(getter, semanticModel, cancellationToken, out result);
                }
            }

            result = null;
            return false;
        }

        internal static bool TryGetBackingFields(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol getter, out IFieldSymbol setter)
        {
            getter = null;
            setter = null;
            if (!property.IsPotentialClrProperty())
            {
                return false;
            }

            SyntaxReference reference;
            if (property.DeclaringSyntaxReferences.TryGetLast(out reference))
            {
                var propertyDeclaration = reference.GetSyntax(cancellationToken) as PropertyDeclarationSyntax;
                if (propertyDeclaration == null)
                {
                    return false;
                }

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
    }
}