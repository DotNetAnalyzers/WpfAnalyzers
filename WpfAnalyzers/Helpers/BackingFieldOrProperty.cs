namespace WpfAnalyzers
{
    using System.Diagnostics;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [DebuggerDisplay("{this.Symbol}")]
    internal struct BackingFieldOrProperty
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

        internal static bool TryCreateForDependencyProperty(ISymbol symbol, out BackingFieldOrProperty result)
        {
            if (symbol != null &&
                symbol.IsStatic &&
                FieldOrProperty.TryCreate(symbol, out var fieldOrProperty) &&
                fieldOrProperty.Type.IsEither(KnownSymbols.DependencyProperty, KnownSymbols.DependencyPropertyKey))
            {
                result = new BackingFieldOrProperty(fieldOrProperty);
                return true;
            }

            result = default;
            return false;
        }

        internal static bool TryCreateCandidate(ISymbol symbol, out BackingFieldOrProperty result)
        {
            if (symbol != null &&
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
            if (member.TryFirstAncestorOrSelf(out PropertyDeclarationSyntax property))
            {
                return property.Identifier;
            }

            if (member.TryFirstAncestorOrSelf(out FieldDeclarationSyntax field))
            {
                if (field.Declaration.Variables.TrySingle(out var variable))
                {
                    return variable.Identifier;
                }
            }

            return member.GetFirstToken();
        }

        internal bool TryGetAssignedValue(CancellationToken cancellationToken, out ExpressionSyntax value)
        {
            return this.FieldOrProperty.TryGetAssignedValue(cancellationToken, out value);
        }

        internal bool TryGetSyntaxReference(out SyntaxReference syntaxReference)
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
    }
}
