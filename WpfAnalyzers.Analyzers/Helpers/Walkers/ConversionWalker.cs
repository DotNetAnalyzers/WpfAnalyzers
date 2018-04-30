namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class ConversionWalker : PooledWalker<ConversionWalker>
    {
        private readonly List<CastExpressionSyntax> casts = new List<CastExpressionSyntax>();
        private readonly List<BinaryExpressionSyntax> asCasts = new List<BinaryExpressionSyntax>();
        private readonly List<BinaryExpressionSyntax> isChecks = new List<BinaryExpressionSyntax>();
        private readonly List<IsPatternExpressionSyntax> isPatterns = new List<IsPatternExpressionSyntax>();
        private readonly List<CasePatternSwitchLabelSyntax> caseLabels = new List<CasePatternSwitchLabelSyntax>();

        private ConversionWalker()
        {
        }

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
                this.isChecks.Add(node);
            }

            base.VisitBinaryExpression(node);
        }

        public override void VisitIsPatternExpression(IsPatternExpressionSyntax node)
        {
            this.isPatterns.Add(node);
            base.VisitIsPatternExpression(node);
        }

        public override void VisitCasePatternSwitchLabel(CasePatternSwitchLabelSyntax node)
        {
            this.caseLabels.Add(node);
            base.VisitCasePatternSwitchLabel(node);
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

            bool TryGetCommonBase(ITypeSymbol t1, TypeSyntax ts, out ITypeSymbol result)
            {
                result = null;
                if (ts == null)
                {
                    return false;
                }

                var t2 = semanticModel.GetTypeInfoSafe(ts, cancellationToken).Type;
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
                    t1.IsAssignableTo(t2, semanticModel.Compilation))
                {
                    result = t2;
                    return true;
                }

                if (t2.IsAssignableTo(t1, semanticModel.Compilation))
                {
                    result = t1;
                    return true;
                }

                using (var set = PooledSet<ITypeSymbol>.Borrow())
                {
                    set.UnionWith(t1.RecursiveBaseTypes());
                    set.IntersectWith(t2.RecursiveBaseTypes());
                    return set.TryFirst(
                               x => x is INamedTypeSymbol namedType &&
                                    namedType.IsGenericType,
                               out result) ||
                           set.TryFirst(out result);
                }
            }

            toType = null;
            using (var walker = Borrow(node))
            {
                foreach (var cast in walker.casts)
                {
                    if (IsFor(cast.Expression, symbol, semanticModel, cancellationToken) &&
                        !TryGetCommonBase(toType, cast.Type, out toType))
                    {
                        return false;
                    }
                }

                foreach (var cast in walker.asCasts)
                {
                    if (IsFor(cast.Left, symbol, semanticModel, cancellationToken) &&
                        !TryGetCommonBase(toType, cast.Right as TypeSyntax, out toType))
                    {
                        return false;
                    }
                }

                foreach (var isCheck in walker.isChecks)
                {
                    if (IsFor(isCheck.Left, symbol, semanticModel, cancellationToken) &&
                        !TryGetCommonBase(toType, isCheck.Right as TypeSyntax, out toType))
                    {
                        return false;
                    }
                }

                foreach (var isPattern in walker.isPatterns)
                {
                    if (isPattern.Pattern is DeclarationPatternSyntax declarationPattern &&
                        IsFor(isPattern.Expression, symbol, semanticModel, cancellationToken) &&
                        !TryGetCommonBase(toType, declarationPattern.Type, out toType))
                    {
                        return false;
                    }
                }

                foreach (var label in walker.caseLabels)
                {
                    if (label.Pattern is DeclarationPatternSyntax declarationPattern &&
                        IsFor(label.FirstAncestor<SwitchStatementSyntax>().Expression, symbol, semanticModel, cancellationToken) &&
                        !TryGetCommonBase(toType, declarationPattern.Type, out toType))
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
            this.isChecks.Clear();
            this.isPatterns.Clear();
            this.caseLabels.Clear();
        }
    }
}
