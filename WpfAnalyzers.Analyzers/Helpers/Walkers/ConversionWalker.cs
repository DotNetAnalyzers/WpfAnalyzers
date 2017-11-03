namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class ConversionWalker : PooledWalker<ConversionWalker>
    {
        private readonly List<CastExpressionSyntax> casts = new List<CastExpressionSyntax>();
        private readonly List<BinaryExpressionSyntax> asCasts = new List<BinaryExpressionSyntax>();
        private readonly List<BinaryExpressionSyntax> isCasts = new List<BinaryExpressionSyntax>();

        private ConversionWalker()
        {
        }

        public IReadOnlyList<CastExpressionSyntax> Casts => this.casts;

        public IReadOnlyList<BinaryExpressionSyntax> AsCasts => this.asCasts;

        public IReadOnlyList<BinaryExpressionSyntax> IsCasts => this.isCasts;

        public override void VisitCastExpression(CastExpressionSyntax node)
        {
            this.casts.Add(node);
            base.VisitCastExpression(node);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.AsExpression))
            {
                this.asCasts.Add(node);
            }

            if (node.IsKind(SyntaxKind.IsExpression))
            {
                this.isCasts.Add(node);
            }

            base.VisitBinaryExpression(node);
        }

        internal static ConversionWalker Borrow(SyntaxNode node) => BorrowAndVisit(node, () => new ConversionWalker());

        internal static bool TryGetSingle(SyntaxNode node, IParameterSymbol parameter, out TypeSyntax toType)
        {
            toType = null;
            using (var walker = Borrow(node))
            {
                foreach (var cast in walker.casts)
                {
                    if (cast.Expression is IdentifierNameSyntax identifierName &&
                        identifierName.Identifier.ValueText == parameter.Name)
                    {
                        if (toType != null)
                        {
                            return false;
                        }

                        toType = cast.Type;
                    }
                }

                foreach (var cast in walker.asCasts)
                {
                    if (cast.Left is IdentifierNameSyntax identifierName &&
                        identifierName.Identifier.ValueText == parameter.Name)
                    {
                        if (toType != null ||
                            !(cast.Right is TypeSyntax typeSyntax))
                        {
                            return false;
                        }

                        toType = typeSyntax;
                    }
                }

                foreach (var cast in walker.isCasts)
                {
                    if (cast.Left is IdentifierNameSyntax identifierName &&
                        identifierName.Identifier.ValueText == parameter.Name)
                    {
                        if (toType != null ||
                            !(cast.Right is TypeSyntax typeSyntax))
                        {
                            return false;
                        }

                        toType = typeSyntax;
                    }
                }

                return toType != null;
            }
        }

        protected override void Clear()
        {
            this.casts.Clear();
            this.asCasts.Clear();
            this.isCasts.Clear();
        }
    }
}