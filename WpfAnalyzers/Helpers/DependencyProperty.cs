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
            if (Register.MatchAny(invocation, semanticModel, cancellationToken) is { } register)
            {
                return (nameArg = register.NameArgument()) is { } &&
                       nameArg.TryGetStringValue(semanticModel, cancellationToken, out registeredName);
            }

            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                (AddOwner.Match(invocation, semanticModel, cancellationToken) is { } ||
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
            if (Register.FindRecursive(backing, semanticModel, cancellationToken) is { } call)
            {
                return (nameArg = call.NameArgument()) is { } &&
                       nameArg.TryGetStringValue(semanticModel, cancellationToken, out result);
            }

            if (TryGetDependencyAddOwnerSourceField(backing, semanticModel, cancellationToken, out var source) &&
                !SymbolEqualityComparer.Default.Equals(source.Symbol, backing.Symbol))
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
            if (Register.FindRecursive(backing, semanticModel, cancellationToken) is { } call)
            {
                result = call.PropertyType(backing.ContainingType, semanticModel, cancellationToken);
                return result is { };
            }

            if (TryGetDependencyAddOwnerSourceField(backing, semanticModel, cancellationToken, out var source) &&
               !SymbolEqualityComparer.Default.Equals(source.Symbol, backing.Symbol))
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

        internal readonly struct Register
        {
            internal readonly InvocationExpressionSyntax Invocation;
            internal readonly IMethodSymbol Target;

            internal Register(InvocationExpressionSyntax invocation, IMethodSymbol target)
            {
                this.Invocation = invocation;
                this.Target = target;
            }

            internal static Register? FindRecursive(BackingFieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (DependencyProperty.TryGetDependencyPropertyKeyFieldOrProperty(fieldOrProperty, semanticModel, cancellationToken, out var keyField))
                {
                    return FindRecursive(keyField, semanticModel, cancellationToken);
                }

                if (DependencyProperty.TryGetDependencyAddOwnerSourceField(fieldOrProperty, semanticModel, cancellationToken, out var addOwnerSource))
                {
                    return FindRecursive(addOwnerSource, semanticModel, cancellationToken);
                }

                return FindRegisterInvocation(fieldOrProperty, semanticModel, cancellationToken);
            }

            internal static Register? FindRegisterInvocation(BackingFieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (fieldOrProperty.TryGetAssignedValue(cancellationToken, out var value) &&
                    value is InvocationExpressionSyntax invocation)
                {
                    return MatchAny(invocation, semanticModel, cancellationToken);
                }

                return null;
            }

            internal static Register? MatchAny(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                return MatchRegister(invocation, semanticModel, cancellationToken) ??
                       MatchRegisterReadOnly(invocation, semanticModel, cancellationToken) ??
                       MatchRegisterAttached(invocation, semanticModel, cancellationToken) ??
                       MatchRegisterAttachedReadOnly(invocation, semanticModel, cancellationToken);
            }

            internal static Register? MatchRegister(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.Register, cancellationToken, out var method))
                {
                    return new Register(invocation, method);
                }

                return null;
            }

            internal static Register? MatchRegisterReadOnly(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.RegisterReadOnly, cancellationToken, out var method))
                {
                    return new Register(invocation, method);
                }

                return null;
            }

            internal static Register? MatchRegisterAttached(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.RegisterAttached, cancellationToken, out var method))
                {
                    return new Register(invocation, method);
                }

                return null;
            }

            internal static Register? MatchRegisterAttachedReadOnly(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.RegisterAttachedReadOnly, cancellationToken, out var method))
                {
                    return new Register(invocation, method);
                }

                return null;
            }

            internal ArgumentSyntax? NameArgument() => this.FindArgument("name");

            internal ArgumentSyntax? PropertyTypeArgument() => this.FindArgument("propertyType");

            internal ArgumentSyntax? FindArgument(string name)
            {
                if (this.Target.TryFindParameter(name, out var parameter))
                {
                    return this.Invocation.FindArgument(parameter);
                }

                return null;
            }

            internal ArgumentSyntax? FindArgument(QualifiedType type)
            {
                if (this.Target.TryFindParameter(type, out var parameter))
                {
                    return this.Invocation.FindArgument(parameter);
                }

                return null;
            }

            internal ITypeSymbol? PropertyType(INamedTypeSymbol containingType, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (this.PropertyTypeArgument() is { Expression: TypeOfExpressionSyntax { Type: { } typeSyntax } } &&
                    TypeSymbol.TryGet(typeSyntax, containingType, semanticModel, cancellationToken) is { } type)
                {
                    if (type.IsReferenceType &&
                        (semanticModel.GetNullableContext(this.Invocation.SpanStart) & NullableContext.Enabled) == NullableContext.Enabled &&
                        IsDefaultValueNull(this.FindArgument(KnownSymbols.PropertyMetadata)))
                    {
                        return type.WithNullableAnnotation(NullableAnnotation.Annotated);
                    }

                    return type;

                    bool IsDefaultValueNull(ArgumentSyntax? metadataArg)
                    {
                        if (metadataArg is { Expression: ObjectCreationExpressionSyntax propertyMetaData } &&
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

                return null;
            }

            internal string? PropertyName(SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                return this.NameArgument() is { } argument &&
                       argument.TryGetStringValue(semanticModel, cancellationToken, out var name)
                    ? name
                    : null;
            }
        }

        internal readonly struct AddOwner
        {
            internal readonly InvocationExpressionSyntax Invocation;
            internal readonly IMethodSymbol Target;

            internal AddOwner(InvocationExpressionSyntax invocation, IMethodSymbol target)
            {
                this.Invocation = invocation;
                this.Target = target;
            }

            internal static AddOwner? Match(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.AddOwner, cancellationToken, out var target))
                {
                    return new AddOwner(invocation, target);
                }

                return null;
            }
        }
    }
}
