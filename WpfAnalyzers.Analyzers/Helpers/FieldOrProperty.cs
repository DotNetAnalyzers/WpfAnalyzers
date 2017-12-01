namespace WpfAnalyzers
{
    using System.Diagnostics;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [DebuggerDisplay("{this.Symbol}")]
    internal struct FieldOrProperty
    {
        private FieldOrProperty(ISymbol symbol)
        {
            this.Symbol = symbol;
        }

        public ISymbol Symbol { get; }

        internal ITypeSymbol Type => (this.Symbol as IFieldSymbol)?.Type ?? ((IPropertySymbol)this.Symbol).Type;

        internal INamedTypeSymbol ContainingType => this.Symbol.ContainingType;

        internal string Name => (this.Symbol as IFieldSymbol)?.Name ?? ((IPropertySymbol)this.Symbol).Name;

        internal static bool TryCreate(ISymbol symbol, out FieldOrProperty result)
        {
            if (symbol != null)
            {
                if (symbol is IFieldSymbol field)
                {
                    result = new FieldOrProperty(field);
                    return true;
                }

                if (symbol is IPropertySymbol property)
                {
                    result = new FieldOrProperty(property);
                    return true;
                }
            }

            result = default(FieldOrProperty);
            return false;
        }

        internal bool TryGetAssignedValue(CancellationToken cancellationToken, out ExpressionSyntax value)
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
    }
}