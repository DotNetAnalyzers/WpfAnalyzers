namespace WpfAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class DependencyProperty
    {
        internal static bool TryGetRegisterCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out IMethodSymbol? method)
        {
            return semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.Register, cancellationToken, out method);
        }

        internal static bool TryGetRegisterReadOnlyCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out IMethodSymbol? method)
        {
            return semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.RegisterReadOnly, cancellationToken, out method);
        }

        internal static bool TryGetRegisterAttachedCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out IMethodSymbol? method)
        {
            return semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.RegisterAttached, cancellationToken, out method);
        }

        internal static bool TryGetRegisterAttachedReadOnlyCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out IMethodSymbol? method)
        {
            return semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.RegisterAttachedReadOnly, cancellationToken, out method);
        }

        internal static bool TryGetAddOwnerCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out IMethodSymbol? method)
        {
            return semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.AddOwner, cancellationToken, out method);
        }

        internal static bool TryGetOverrideMetadataCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out IMethodSymbol? method)
        {
            return semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.OverrideMetadata, cancellationToken, out method);
        }

        internal static bool IsPotentialDependencyPropertyBackingField(BackingFieldOrProperty fieldOrProperty)
        {
            return fieldOrProperty.Type == KnownSymbols.DependencyProperty;
        }

        internal static bool IsPotentialDependencyPropertyKeyBackingField(BackingFieldOrProperty fieldOrProperty)
        {
            return fieldOrProperty.Type == KnownSymbols.DependencyPropertyKey;
        }

        internal static bool TryGetRegisteredName(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ArgumentSyntax? nameArg, [NotNullWhen(true)] out string? registeredName)
        {
            nameArg = null;
            registeredName = null;
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

        internal static bool TryGetRegisteredName(BackingFieldOrProperty backing, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ArgumentSyntax? nameArg, [NotNullWhen(true)] out string? result)
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
                !Microsoft.CodeAnalysis.SymbolEqualityComparer.Default.Equals(source.Symbol, backing.Symbol))
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

        internal static bool TryGetRegisteredType(BackingFieldOrProperty backing, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ITypeSymbol? result)
        {
            result = null;
            if (TryGetRegisterInvocationRecursive(backing, semanticModel, cancellationToken, out var invocation, out var method))
            {
                if (invocation.TryGetArgumentAtIndex(1, out var typeArg) &&
                    typeArg.Expression is TypeOfExpressionSyntax { Type: { } typeSyntax } &&
                    TypeSymbol.TryGet(typeSyntax, backing.ContainingType, semanticModel, cancellationToken) is { } type)
                {
                    if (type.IsReferenceType &&
                        (semanticModel.GetNullableContext(invocation.SpanStart) & NullableContext.Enabled) == NullableContext.Enabled &&
                        IsDefaultValueNull())
                    {
                        result = type.WithNullableAnnotation(NullableAnnotation.Annotated);
                        return true;
                    }

                    result = type;
                    return true;

                    bool IsDefaultValueNull()
                    {
                        if (method.TryFindParameter(KnownSymbols.PropertyMetadata, out var parameter) &&
                            invocation.TryFindArgument(parameter, out var metadataArg) &&
                            metadataArg is { Expression: ObjectCreationExpressionSyntax propertyMetaData } &&
                            PropertyMetadata.TryGetDefaultValue(propertyMetaData, semanticModel, cancellationToken, out var defaultValue))
                        {
                            return defaultValue switch
                            {
                                { Expression: DefaultExpressionSyntax _ } => true,
                                { Expression: LiteralExpressionSyntax { Token: { ValueText: "null" } } } => true,
                                { Expression: CastExpressionSyntax { Expression: LiteralExpressionSyntax { Token: { ValueText: "null" } } } } => true,
                                _ => false,
                            };
                        }

                        return true;
                    }
                }

                return false;
            }

            if (TryGetDependencyAddOwnerSourceField(backing, semanticModel, cancellationToken, out var source) &&
               !Microsoft.CodeAnalysis.SymbolEqualityComparer.Default.Equals(source.Symbol, backing.Symbol))
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
                semanticModel.TryGetSymbol(value, cancellationToken, out var symbol))
            {
                if (symbol is IMethodSymbol method)
                {
                    return method == KnownSymbols.DependencyProperty.AddOwner &&
                           value is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: { } expression } } &&
                           semanticModel.TryGetSymbol(expression, cancellationToken, out var candidate) &&
                           BackingFieldOrProperty.TryCreateForDependencyProperty(candidate, out result) &&
                           TryGetDependencyPropertyKeyFieldOrProperty(result, semanticModel, cancellationToken, out result);
                }
                else
                {
                    return symbol is IPropertySymbol property &&
                           property == KnownSymbols.DependencyPropertyKey.DependencyProperty &&
                           value is MemberAccessExpressionSyntax { Expression: { } expression } &&
                           semanticModel.TryGetSymbol(expression, cancellationToken, out var candidate) &&
                           BackingFieldOrProperty.TryCreateForDependencyProperty(candidate, out result);
                }
            }

            return false;
        }

        internal static bool TryGetDependencyAddOwnerSourceField(BackingFieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty result)
        {
            result = default;

            return fieldOrProperty.TryGetAssignedValue(cancellationToken, out var value) &&
                   value is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: { } addOwner } } invocation &&
                   semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.AddOwner, cancellationToken, out _) &&
                   semanticModel.TryGetSymbol(addOwner, cancellationToken, out var addOwnerSymbol) &&
                   BackingFieldOrProperty.TryCreateForDependencyProperty(addOwnerSymbol, out result);
        }

        internal static bool TryGetRegisterInvocation(BackingFieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out InvocationExpressionSyntax? result, [NotNullWhen(true)] out IMethodSymbol? symbol)
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

        internal static bool TryGetRegisterInvocationRecursive(BackingFieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out InvocationExpressionSyntax? result, [NotNullWhen(true)] out IMethodSymbol? symbol)
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

        internal static bool TryGetPropertyByName(BackingFieldOrProperty fieldOrProperty, [NotNullWhen(true)] out IPropertySymbol? property)
        {
            if (Suffix() is { } suffix)
            {
                foreach (var symbol in fieldOrProperty.ContainingType.GetMembers())
                {
                    if (symbol is IPropertySymbol candidate &&
                        fieldOrProperty.Name.IsParts(candidate.Name, suffix))
                    {
                        property = candidate;
                        return true;
                    }
                }
            }

            property = null;
            return false;

            string? Suffix()
            {
                if (IsPotentialDependencyPropertyBackingField(fieldOrProperty))
                {
                    return "Property";
                }

                return IsPotentialDependencyPropertyBackingField(fieldOrProperty)
                    ? "PropertyKey"
                    : null;
            }
        }
    }
}
