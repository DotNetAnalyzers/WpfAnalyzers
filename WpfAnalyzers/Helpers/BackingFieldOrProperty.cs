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

        internal static bool TryCreateForDependencyProperty(ISymbol? symbol, out BackingFieldOrProperty result)
        {
            if (symbol is { IsStatic: true } &&
                FieldOrProperty.TryCreate(symbol, out var fieldOrProperty) &&
                fieldOrProperty.Type.IsEither(KnownSymbols.DependencyProperty, KnownSymbols.DependencyPropertyKey))
            {
                result = new BackingFieldOrProperty(fieldOrProperty);
                return true;
            }

            result = default;
            return false;
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
            if (DependencyProperty.Register.FindRecursive(this, semanticModel, cancellationToken) is { } register)
            {
                if (register.NameArgument() is { } argument)
                {
                    if (argument.TryGetStringValue(semanticModel, cancellationToken, out var name))
                    {
                        return new ArgumentAndValue<string?>(argument, name);
                    }

                    return new ArgumentAndValue<string?>(argument, name);
                }

                return null;
            }

            if (DependencyProperty.TryGetDependencyAddOwnerSourceField(this, semanticModel, cancellationToken, out var source) &&
                !SymbolEqualityComparer.Default.Equals(source.Symbol, this.Symbol))
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
            if (DependencyProperty.Register.FindRecursive(this, semanticModel, cancellationToken) is { } register)
            {
                if (register.PropertyTypeArgument() is { } argument)
                {
                    if (register.PropertyType(this.ContainingType, semanticModel, cancellationToken) is {} type)
                    {
                        return new ArgumentAndValue<ITypeSymbol?>(argument, type);
                    }

                    return new ArgumentAndValue<ITypeSymbol?>(argument, null);
                }

                return null;
            }

            if (DependencyProperty.TryGetDependencyAddOwnerSourceField(this, semanticModel, cancellationToken, out var source) &&
                !SymbolEqualityComparer.Default.Equals(source.Symbol, this.Symbol))
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

        internal bool TryGetAssignedValue(CancellationToken cancellationToken, [NotNullWhen(true)] out ExpressionSyntax? value)
        {
            return this.FieldOrProperty.TryGetAssignedValue(cancellationToken, out value);
        }

        internal bool TryGetSyntaxReference([NotNullWhen(true)] out SyntaxReference? syntaxReference)
        {
            return this.Symbol.DeclaringSyntaxReferences.TrySingle(out syntaxReference);
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
