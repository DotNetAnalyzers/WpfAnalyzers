namespace WpfAnalyzers
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [DebuggerDisplay("{this.Symbol}")]
    internal readonly struct BackingFieldOrProperty
    {
        private BackingFieldOrProperty(FieldOrProperty fieldOrProperty)
        {
            this.FieldOrProperty = fieldOrProperty;
        }

        internal FieldOrProperty FieldOrProperty { get; }

        internal ISymbol Symbol => this.FieldOrProperty.Symbol;

        internal ITypeSymbol Type => this.FieldOrProperty.Type;

        internal INamedTypeSymbol ContainingType => this.FieldOrProperty.ContainingType;

        internal string Name => this.FieldOrProperty.Name;

        internal static BackingFieldOrProperty? Match(ISymbol? symbol)
        {
            if (symbol is { IsStatic: true } &&
                FieldOrProperty.TryCreate(symbol, out var fieldOrProperty) &&
                fieldOrProperty.Type.IsEither(KnownSymbols.DependencyProperty, KnownSymbols.DependencyPropertyKey))
            {
                return new BackingFieldOrProperty(fieldOrProperty);
            }

            return null;
        }

        internal static bool TryCreateCandidate(ISymbol? symbol, out BackingFieldOrProperty result)
        {
            if (symbol is { } &&
                FieldOrProperty.TryCreate(symbol, out var fieldOrProperty) &&
                fieldOrProperty.Type.IsEither(KnownSymbols.DependencyProperty, KnownSymbols.DependencyPropertyKey))
            {
                result = new BackingFieldOrProperty(fieldOrProperty);
                return true;
            }

            result = default;
            return false;
        }

        internal static SyntaxToken FindIdentifier(MemberDeclarationSyntax member)
        {
            if (member.TryFirstAncestorOrSelf(out PropertyDeclarationSyntax? property))
            {
                return property.Identifier;
            }

            if (member.TryFirstAncestorOrSelf(out FieldDeclarationSyntax? field))
            {
                if (field.Declaration.Variables.TrySingle(out var variable))
                {
                    return variable.Identifier;
                }
            }

            return member.GetFirstToken();
        }

        internal ArgumentAndValue<string?>? RegisteredName(SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (DependencyProperty.Register.FindRecursive(this, semanticModel, cancellationToken) is { NameArgument: { } nameArgument })
            {
                if (nameArgument.TryGetStringValue(semanticModel, cancellationToken, out var name))
                {
                    return new ArgumentAndValue<string?>(nameArgument, name);
                }

                return new ArgumentAndValue<string?>(nameArgument, name);
            }

            if (this.FindAddOwnerSource(semanticModel, cancellationToken) is { } source &&
                !SymbolComparer.Equal(source.Symbol, this.Symbol))
            {
                return source.RegisteredName(semanticModel, cancellationToken);
            }

            if (this.Symbol.Locations.All(x => !x.IsInSource) &&
                this.PropertyByName() is { Name: { } } match)
            {
                return new ArgumentAndValue<string?>(null, match.Name);
            }

            return null;
        }

        internal ArgumentAndValue<ITypeSymbol?>? RegisteredType(SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (DependencyProperty.Register.FindRecursive(this, semanticModel, cancellationToken) is { PropertyTypeArgument: { } propertyTypeArgument } register)
            {
                if (register.PropertyType(this.ContainingType, semanticModel, cancellationToken) is { } type)
                {
                    return new ArgumentAndValue<ITypeSymbol?>(propertyTypeArgument, type);
                }

                return new ArgumentAndValue<ITypeSymbol?>(propertyTypeArgument, null);
            }

            if (this.FindAddOwnerSource(semanticModel, cancellationToken) is { } source &&
                !SymbolComparer.Equal(source.Symbol, this.Symbol))
            {
                return source.RegisteredType(semanticModel, cancellationToken);
            }

            if (this.Symbol.Locations.All(x => !x.IsInSource) &&
                this.PropertyByName() is { Name: { } } match)
            {
                return new ArgumentAndValue<ITypeSymbol?>(null, match.Type);
            }

            return null;
        }

        internal ExpressionSyntax? Value(CancellationToken cancellationToken)
        {
            return this.FieldOrProperty.Value(cancellationToken);
        }

        internal bool TryGetSyntaxReference([NotNullWhen(true)] out SyntaxReference? syntaxReference)
        {
            return this.Symbol.DeclaringSyntaxReferences.TrySingle(out syntaxReference);
        }

        internal BackingFieldOrProperty? FindKey(SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (this.Value(cancellationToken) is { } value &&
                semanticModel.TryGetSymbol(value, cancellationToken, out var symbol))
            {
                return symbol switch
                {
                    IMethodSymbol method
                        when method == KnownSymbols.DependencyProperty.AddOwner &&
                             value is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: { } expression } } &&
                             semanticModel.TryGetSymbol(expression, cancellationToken, out var candidate) &&
                             Match(candidate) is { } match
                        => match.FindKey(semanticModel, cancellationToken),
                    IPropertySymbol property
                        when property == KnownSymbols.DependencyPropertyKey.DependencyProperty &&
                             value is MemberAccessExpressionSyntax { Expression: { } expression } &&
                             semanticModel.TryGetSymbol(expression, cancellationToken, out var candidate)
                         => Match(candidate),
                    _ => null,
                };
            }

            return null;
        }

        internal BackingFieldOrProperty? FindAddOwnerSource(SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (this.Value(cancellationToken) is { } value &&
                   value is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: { } addOwner } } invocation &&
                   semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyProperty.AddOwner, cancellationToken, out _) &&
                   semanticModel.TryGetSymbol(addOwner, cancellationToken, out var addOwnerSymbol))
            {
                return Match(addOwnerSymbol);
            }

            return null;
        }

        internal ArgumentSyntax CreateArgument(SemanticModel semanticModel, int position)
        {
            var name = this.Name;
            if (semanticModel.LookupStaticMembers(position, name: name).Contains(this.Symbol))
            {
                return SyntaxFactory.Argument(SyntaxFactory.IdentifierName(name));
            }

            var typeName = this.ContainingType.ToMinimalDisplayString(semanticModel, position, SymbolDisplayFormat.MinimallyQualifiedFormat);
            return SyntaxFactory.Argument(SyntaxFactory.ParseExpression($"{typeName}.{name}"));
        }

        private IPropertySymbol? PropertyByName()
        {
            if (Suffix(this.Type) is { } suffix)
            {
                foreach (var symbol in this.ContainingType.GetMembers())
                {
                    if (symbol is IPropertySymbol candidate &&
                        this.Name.IsParts(candidate.Name, suffix))
                    {
                        return candidate;
                    }
                }
            }

            return null;

            static string? Suffix(ITypeSymbol type)
            {
                if (type == KnownSymbols.DependencyProperty)
                {
                    return "Property";
                }

                if (type == KnownSymbols.DependencyPropertyKey)
                {
                    return "PropertyKey";
                }

                return null;
            }
        }
    }
}
