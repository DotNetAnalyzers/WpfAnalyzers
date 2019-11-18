namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Exposes helper methods for working with CLR-properties for DependencyProperty.
    /// </summary>
    internal static class ClrProperty
    {
        /// <summary>
        /// Check if the <paramref name="property"/> can be an accessor for a DependencyProperty.
        /// </summary>
        internal static bool IsPotentialClrProperty(this IPropertySymbol property, Compilation compilation)
        {
            return property is { IsIndexer: false, IsReadOnly: false, IsWriteOnly: false, IsStatic: false } &&
                   property.ContainingType.IsAssignableTo(KnownSymbols.DependencyObject, compilation);
        }

        /// <summary>
        /// Check if the <paramref name="property"/> is a CLR accessor for a DependencyProperty.
        /// </summary>
        internal static bool IsDependencyPropertyAccessor(this IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return property.IsPotentialClrProperty(semanticModel.Compilation) &&
                   property.TrySingleDeclaration(cancellationToken, out PropertyDeclarationSyntax? propertyDeclaration) &&
                   IsDependencyPropertyAccessor(propertyDeclaration, semanticModel, cancellationToken);
        }

        /// <summary>
        /// Check if the <paramref name="property"/> is a CLR accessor for a DependencyProperty.
        /// </summary>
        internal static bool IsDependencyPropertyAccessor(PropertyDeclarationSyntax property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return TryGetBackingFields(property, semanticModel, cancellationToken, out var _, out var _);
        }

        /// <summary>
        /// Get the single DependencyProperty backing field for <paramref name="property"/>
        /// Returns false for accessors for readonly dependency properties.
        /// </summary>
        internal static bool TrySingleBackingField(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty result)
        {
            result = default;
            BackingFieldOrProperty getter;
            BackingFieldOrProperty setter;
            if (property.IsPotentialClrProperty(semanticModel.Compilation) &&
                property.TrySingleDeclaration(cancellationToken, out _))
            {
                if (TryGetBackingFields(
                    property,
                    semanticModel,
                    cancellationToken,
                    out getter,
                    out setter))
                {
                    if (ReferenceEquals(setter.Symbol, getter.Symbol) &&
                        setter.Type == KnownSymbols.DependencyProperty)
                    {
                        result = setter;
                        return true;
                    }
                }
            }

            if (TryGetBackingFieldsByName(property, semanticModel.Compilation, out getter, out setter))
            {
                if (ReferenceEquals(getter.Symbol, setter.Symbol))
                {
                    result = getter;
                    return result.Type == KnownSymbols.DependencyProperty;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the backing fields for the <paramref name="propertyDeclaration"/> these are different for readonly dependency properties where the setter returns the DependencyPropertyKey field.
        /// </summary>
        internal static bool TryGetBackingFields(PropertyDeclarationSyntax propertyDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty getField, out BackingFieldOrProperty setField)
        {
            getField = default;
            setField = default;
            if (propertyDeclaration.TryGetGetter(out var getAccessor) &&
                propertyDeclaration.TryGetSetter(out var setAccessor))
            {
                using var getterWalker = ClrGetterWalker.Borrow(semanticModel, getAccessor, cancellationToken);
                using var setterWalker = ClrSetterWalker.Borrow(semanticModel, setAccessor, cancellationToken);
                if (getterWalker.HasError ||
                    setterWalker.HasError)
                {
                    return false;
                }

                if (getterWalker.IsSuccess &&
                    getterWalker.Property?.Expression is { } getExpression &&
                    semanticModel.TryGetSymbol(getExpression, cancellationToken, out var symbol) &&
                    BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out getField) &&
                    setterWalker.IsSuccess &&
                    setterWalker.Property?.Expression is { } setExpression &&
                    semanticModel.TryGetSymbol(setExpression, cancellationToken, out symbol) &&
                    BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out setField))
                {
                    return true;
                }

                return semanticModel.TryGetSymbol(propertyDeclaration, cancellationToken, out var property) &&
                       TryGetBackingFieldsByName(property, semanticModel.Compilation, out getField, out setField);
            }

            return false;
        }

        internal static bool TryGetRegisterField(PropertyDeclarationSyntax property, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty result)
        {
            result = default;
            if (TryGetBackingFields(property, semanticModel, cancellationToken, out var getter, out var setter))
            {
                if (DependencyProperty.TryGetDependencyPropertyKeyFieldOrProperty(getter, semanticModel, cancellationToken, out var keyField))
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
        /// Get the backing fields for the <paramref name="property"/> these are different for readonly dependency properties where the setter returns the DependencyPropertyKey field.
        /// </summary>
        private static bool TryGetBackingFields(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty getField, out BackingFieldOrProperty setField)
        {
            getField = default;
            setField = default;

            if (property.IsPotentialClrProperty(semanticModel.Compilation) &&
                property.TrySingleDeclaration(cancellationToken, out PropertyDeclarationSyntax? propertyDeclaration) &&
                TryGetBackingFields(propertyDeclaration, semanticModel, cancellationToken, out getField, out setField))
            {
                if (getField.ContainingType.IsGenericType)
                {
                    return property.ContainingType.TryFindFirstMember(getField.Name, out var getMember) &&
                           BackingFieldOrProperty.TryCreateForDependencyProperty(getMember, out getField) &&
                           property.ContainingType.TryFindFirstMember(setField.Name, out var setMember) &&
                           BackingFieldOrProperty.TryCreateForDependencyProperty(setMember, out setField);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the backing fields for the <paramref name="property"/> these are different for readonly dependency properties where the setter returns the DependencyPropertyKey field.
        /// This method looks for fields that matches the name NameProperty and NamePropertyKey.
        /// </summary>
        private static bool TryGetBackingFieldsByName(IPropertySymbol property, Compilation compilation, out BackingFieldOrProperty getter, out BackingFieldOrProperty setter)
        {
            getter = default;
            setter = default;
            if (property == null ||
                !property.ContainingType.IsAssignableTo(KnownSymbols.DependencyObject, compilation))
            {
                return false;
            }

            foreach (var member in property.ContainingType.GetMembers())
            {
                if (BackingFieldOrProperty.TryCreateForDependencyProperty(member, out var candidate))
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
    }
}
