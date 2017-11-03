namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class ConversionWalker : PooledWalker<ConversionWalker>
    {
        private readonly List<CastExpressionSyntax> casts = new List<CastExpressionSyntax>();

        private ConversionWalker()
        {
        }

        public IReadOnlyList<CastExpressionSyntax> Casts => this.casts;

        public static ConversionWalker Borrow(SyntaxNode node) => BorrowAndVisit(node, () => new ConversionWalker());

        public override void VisitCastExpression(CastExpressionSyntax node)
        {
            this.casts.Add(node);
            base.VisitCastExpression(node);
        }

        protected override void Clear()
        {
            this.casts.Clear();
        }
    }
}