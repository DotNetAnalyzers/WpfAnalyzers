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
        internal readonly InvocationExpressionSyntax? GetValue;
        internal readonly InvocationExpressionSyntax? SetValue;

        private ClrProperty(BackingFieldOrProperty backingGet, BackingFieldOrProperty backingSet, InvocationExpressionSyntax? getValue, InvocationExpressionSyntax? setValue)
        {
            this.BackingGet = backingGet;
            this.BackingSet = backingSet;
            this.GetValue = getValue;
            this.SetValue = setValue;
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
                        if (DependencyObject.GetValue.Find(MethodOrAccessor.Create(getter), semanticModel, cancellationToken) is { Invocation: { } getValue, PropertyArgument: { Expression: { } getProperty } } &&
                            semanticModel.TryGetSymbol(getProperty, cancellationToken, out var symbol) &&
                            BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out var backingGet) &&
                            DependencyObject.SetValue.Find(MethodOrAccessor.Create(setter), semanticModel, cancellationToken) is { Invocation: { } setValue, PropertyArgument: { Expression: { } setProperty } } &&
                            semanticModel.TryGetSymbol(setProperty, cancellationToken, out symbol) &&
                            BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out var setField))
                        {
                            return Create(property.ContainingType, backingGet, setField, getValue, setValue);
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
                            if (candidate.Type != KnownSymbols.DependencyProperty)
                            {
                                return null;
                            }

                            getField = candidate;
                        }

                        if (candidate.Name.IsParts(property.Name, "PropertyKey"))
                        {
                            if (candidate.Type != KnownSymbols.DependencyPropertyKey)
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

            static ClrProperty? Create(INamedTypeSymbol containingType, BackingFieldOrProperty backingGet, BackingFieldOrProperty backingSet, InvocationExpressionSyntax? getValue, InvocationExpressionSyntax? setValue)
            {
                if (!TypeSymbolComparer.Equal(containingType, backingGet.ContainingType) &&
                    backingGet.ContainingType.IsGenericType)
                {
                    if (containingType.TryFindFirstMember(backingGet.Name, out var getMember) &&
                        BackingFieldOrProperty.TryCreateForDependencyProperty(getMember, out backingGet) &&
                        containingType.TryFindFirstMember(backingSet.Name, out var setMember) &&
                        BackingFieldOrProperty.TryCreateForDependencyProperty(setMember, out backingSet))
                    {
                        return new ClrProperty(backingGet, backingSet, getValue, setValue);
                    }

                    return null;
                }

                return new ClrProperty(backingGet, backingSet, getValue, setValue);
            }
        }
    }
}
