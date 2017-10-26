namespace WpfAnalyzers
{
    using System.Diagnostics;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [DebuggerDisplay("{this.Symbol}")]
    internal struct BackingFieldOrProperty
    {
        private BackingFieldOrProperty(ISymbol symbol)
        {
            this.Symbol = symbol;
        }

        public ISymbol Symbol { get; }

        internal ITypeSymbol Type => (this.Symbol as IFieldSymbol)?.Type ?? ((IPropertySymbol)this.Symbol).Type;

        internal INamedTypeSymbol ContainingType => this.Symbol.ContainingType;

        internal string Name => (this.Symbol as IFieldSymbol)?.Name ?? ((IPropertySymbol)this.Symbol).Name;

        internal static bool TryCreate(ISymbol symbol, out BackingFieldOrProperty result)
        {
            if (symbol != null &&
                symbol.IsStatic)
            {
                if (symbol is IFieldSymbol field &&
                    field.Type.IsEither(KnownSymbol.DependencyProperty, KnownSymbol.DependencyPropertyKey))
                {
                    result = new BackingFieldOrProperty(field);
                    return true;
                }

                if (symbol is IPropertySymbol property &&
                    property.Type.IsEither(KnownSymbol.DependencyProperty, KnownSymbol.DependencyPropertyKey))
                {
                    result = new BackingFieldOrProperty(property);
                    return true;
                }
            }

            result = default(BackingFieldOrProperty);
            return false;
        }

        internal static bool TryCreateCandidate(ISymbol symbol, out BackingFieldOrProperty result)
        {
            if (symbol != null)
            {
                if (symbol is IFieldSymbol field &&
                    field.Type.IsEither(KnownSymbol.DependencyProperty, KnownSymbol.DependencyPropertyKey))
                {
                    result = new BackingFieldOrProperty(field);
                    return true;
                }

                if (symbol is IPropertySymbol property &&
                    property.Type.IsEither(KnownSymbol.DependencyProperty, KnownSymbol.DependencyPropertyKey))
                {
                    result = new BackingFieldOrProperty(property);
                    return true;
                }
            }

            result = default(BackingFieldOrProperty);
            return false;
        }

        internal bool TryGetValue(CancellationToken cancellationToken, out ExpressionSyntax value)
        {
            value = null;
            if (this.Symbol is IFieldSymbol field)
            {
                return field.TryGetAssignedValue(cancellationToken, out value);
            }

            if (this.Symbol is IPropertySymbol property)
            {
                if (property.TryGetSingleDeclaration(cancellationToken, out var declaration))
                {
                    if (declaration.Initializer != null)
                    {
                        value = declaration.Initializer.Value;
                    }
                    else if (declaration.ExpressionBody != null)
                    {
                        value = declaration.ExpressionBody.Expression;
                    }

                    return value != null;
                }
            }

            return false;
        }

        internal bool TryGetSyntaxReference(out SyntaxReference syntaxReference)
        {
            return this.Symbol.DeclaringSyntaxReferences.TryGetSingle(out syntaxReference);
        }

        internal SyntaxToken FindIdentifier(SyntaxNode node)
        {
            if (node is PropertyDeclarationSyntax propertyDeclaration)
            {
                return propertyDeclaration.Identifier;
            }

            if (node is FieldDeclarationSyntax fieldDeclaration)
            {
                if (fieldDeclaration.Declaration.Variables.TryGetSingle(out var variable))
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
