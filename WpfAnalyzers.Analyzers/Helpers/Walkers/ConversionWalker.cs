namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;
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

        internal static bool TryGetCommonBase(SyntaxNode node, ISymbol symbol, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol toType)
        {
            bool IsFor(ExpressionSyntax e, ISymbol s, SemanticModel sm, CancellationToken ct)
            {
                if (e is IdentifierNameSyntax idn &&
                    idn.Identifier.ValueText != symbol.Name)
                {
                    return false;
                }

                return ReferenceEquals(sm.GetSymbolSafe(e, ct), s);
            }

            bool TryGetCommonBase(ITypeSymbol t1, TypeSyntax ts, SemanticModel sm, CancellationToken ct, out ITypeSymbol result)
            {
                result = null;
                if (ts == null)
                {
                    return false;
                }

                var t2 = sm.GetTypeInfoSafe(ts, ct).Type;
                if (t2 == null)
                {
                    return false;
                }

                if (ReferenceEquals(t1, t2))
                {
                    result = t1;
                    return true;
                }

                if (t1 == null ||
                    t1.Is(t2))
                {
                    result = t2;
                    return true;
                }

                if (t2.Is(t1))
                {
                    result = t1;
                    return true;
                }

                using (var set = PooledHashSet<ITypeSymbol>.Borrow())
                {
                    set.UnionWith(t1.RecursiveBaseTypes());
                    set.IntersectWith(t2.RecursiveBaseTypes());
                    return set.TryGetFirst(
                               x => x is INamedTypeSymbol namedType &&
                                    namedType.IsGenericType,
                               out result) ||
                           set.TryGetFirst(out result);
                }
            }

            toType = null;
            using (var walker = Borrow(node))
            {
                foreach (var cast in walker.casts)
                {
                    if (IsFor(cast.Expression, symbol, semanticModel, cancellationToken) &&
                        !TryGetCommonBase(toType, cast.Type, semanticModel, cancellationToken, out toType))
                    {
                        return false;
                    }
                }

                foreach (var cast in walker.asCasts)
                {
                    if (IsFor(cast.Left, symbol, semanticModel, cancellationToken) &&
                        !TryGetCommonBase(toType, cast.Right as TypeSyntax, semanticModel, cancellationToken, out toType))
                    {
                        return false;
                    }
                }

                foreach (var cast in walker.isCasts)
                {
                    if (IsFor(cast.Left, symbol, semanticModel, cancellationToken) &&
                        !TryGetCommonBase(toType, cast.Right as TypeSyntax, semanticModel, cancellationToken, out toType))
                    {
                        return false;
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