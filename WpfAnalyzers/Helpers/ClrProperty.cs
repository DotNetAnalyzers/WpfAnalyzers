namespace WpfAnalyzers
{
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Exposes helper methods for working with CLR-properties for DependencyProperty.
    /// </summary>
    internal readonly struct ClrProperty
    {
        internal readonly BackingFieldOrProperty BackingGet;
        internal readonly BackingFieldOrProperty BackingSet;
        internal readonly AccessorDeclarationSyntax? Getter;
        internal readonly AccessorDeclarationSyntax? Setter;

        private ClrProperty(BackingFieldOrProperty backingGet, BackingFieldOrProperty backingSet, AccessorDeclarationSyntax? getter, AccessorDeclarationSyntax? setter)
        {
            this.BackingGet = backingGet;
            this.BackingSet = backingSet;
            this.Getter = getter;
            this.Setter = setter;
        }

        /// <summary>
        /// Get the single DependencyProperty backing field for <paramref name="property"/>
        /// Returns false for accessors for readonly dependency properties.
        /// </summary>
        internal static ClrProperty? Match(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (property is { IsIndexer: false, IsReadOnly: false, IsWriteOnly: false, IsStatic: false } &&
                property.ContainingType.IsAssignableTo(KnownSymbols.DependencyObject, semanticModel.Compilation))
            {
                if (property.TrySingleDeclaration(cancellationToken, out PropertyDeclarationSyntax? propertyDeclaration))
                {
                    if (propertyDeclaration.Getter() is { } getter &&
                        propertyDeclaration.Setter() is { } setter)
                    {
                        using var getterWalker = ClrGetterWalker.Borrow(semanticModel, getter, cancellationToken);
                        using var setterWalker = ClrSetterWalker.Borrow(semanticModel, setter, cancellationToken);
                        if (getterWalker is { HasError: false, IsSuccess: true, Property: { Expression: { } getExpression } } &&
                            semanticModel.TryGetSymbol(getExpression, cancellationToken, out var symbol) &&
                            BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out var getField) &&
                            setterWalker is { HasError: false, IsSuccess: true, Property: { Expression: { } setExpression } } &&
                            semanticModel.TryGetSymbol(setExpression, cancellationToken, out symbol) &&
                            BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out var setField))
                        {
                            return Create(property.ContainingType, getField, setField, getter, setter);
                        }
                    }

                    return null;
                }

                return CreateByName(property);
            }

            return null;

            static ClrProperty? CreateByName(IPropertySymbol property)
            {
                BackingFieldOrProperty? getField = null;
                BackingFieldOrProperty? setField = null;
                foreach (var member in property.ContainingType.GetMembers())
                {
                    if (BackingFieldOrProperty.TryCreateForDependencyProperty(member, out var candidate))
                    {
                        if (candidate.Name.IsParts(property.Name, "Property"))
                        {
                            if (!DependencyProperty.IsPotentialDependencyPropertyBackingField(candidate))
                            {
                                return null;
                            }

                            getField = candidate;
                        }

                        if (candidate.Name.IsParts(property.Name, "PropertyKey"))
                        {
                            if (!DependencyProperty.IsPotentialDependencyPropertyKeyBackingField(candidate))
                            {
                                return null;
                            }

                            setField = candidate;
                        }
                    }
                }

                if (getField is null)
                {
                    return null;
                }

                setField ??= getField;
                return Create(property.ContainingType, getField.Value, setField.Value, null, null);
            }

            static ClrProperty? Create(INamedTypeSymbol containingType, BackingFieldOrProperty getField, BackingFieldOrProperty setField, AccessorDeclarationSyntax? getter, AccessorDeclarationSyntax? setter)
            {
                if (!TypeSymbolComparer.Equal(containingType, getField.ContainingType) &&
                    getField.ContainingType.IsGenericType)
                {
                    if (containingType.TryFindFirstMember(getField.Name, out var getMember) &&
                        BackingFieldOrProperty.TryCreateForDependencyProperty(getMember, out getField) &&
                        containingType.TryFindFirstMember(setField.Name, out var setMember) &&
                        BackingFieldOrProperty.TryCreateForDependencyProperty(setMember, out setField))
                    {
                        return new ClrProperty(getField, setField, getter, setter);
                    }

                    return null;
                }

                return new ClrProperty(getField, setField, getter, setter);
            }
        }
    }
}
