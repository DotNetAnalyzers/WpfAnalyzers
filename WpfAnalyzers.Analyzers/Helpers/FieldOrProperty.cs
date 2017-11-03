namespace WpfAnalyzers
{
    using System.Diagnostics;
    using Microsoft.CodeAnalysis;

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
    }
}