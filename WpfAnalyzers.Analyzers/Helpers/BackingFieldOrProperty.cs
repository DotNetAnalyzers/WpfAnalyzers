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

        public FieldOrProperty FieldOrProperty { get; }

        public ISymbol Symbol => this.FieldOrProperty.Symbol;

        internal ITypeSymbol Type => this.FieldOrProperty.Type;

        internal INamedTypeSymbol ContainingType => this.FieldOrProperty.ContainingType;

        internal string Name => this.FieldOrProperty.Name;

        internal static bool TryCreate(ISymbol symbol, out BackingFieldOrProperty result)
        {
            if (symbol != null &&
                symbol.IsStatic &&
                FieldOrProperty.TryCreate(symbol, out var fieldOrProperty) &&
                fieldOrProperty.Type.IsEither(KnownSymbol.DependencyProperty, KnownSymbol.DependencyPropertyKey))
            {
                result = new BackingFieldOrProperty(fieldOrProperty);
                return true;
            }

            result = default(BackingFieldOrProperty);
            return false;
        }

        internal static bool TryCreateCandidate(ISymbol symbol, out BackingFieldOrProperty result)
        {
            if (symbol != null &&
                FieldOrProperty.TryCreate(symbol, out var fieldOrProperty) &&
                fieldOrProperty.Type.IsEither(KnownSymbol.DependencyProperty, KnownSymbol.DependencyPropertyKey))
            {
                result = new BackingFieldOrProperty(fieldOrProperty);
                return true;
            }

            result = default(BackingFieldOrProperty);
            return false;
        }

        internal bool TryGetAssignedValue(CancellationToken cancellationToken, out ExpressionSyntax value)
        {
            return this.FieldOrProperty.TryGetAssignedValue(cancellationToken, out value);
        }

        internal bool TryGetSyntaxReference(out SyntaxReference syntaxReference)
        {
            return this.Symbol.DeclaringSyntaxReferences.TrySingle(out syntaxReference);
        }

        internal SyntaxToken FindIdentifier(SyntaxNode node)
        {
            if (node is PropertyDeclarationSyntax propertyDeclaration)
            {
                return propertyDeclaration.Identifier;
            }

            if (node is FieldDeclarationSyntax fieldDeclaration)
            {
                if (fieldDeclaration.Declaration.Variables.TrySingle(out var variable))
                {
                    return variable.Identifier;
                }
            }

            return node.GetFirstToken();
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
