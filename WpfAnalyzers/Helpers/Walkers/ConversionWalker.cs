namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class ConversionWalker : PooledWalker<ConversionWalker>
    {
        private readonly List<CastExpressionSyntax> casts = new();
        private readonly List<BinaryExpressionSyntax> asCasts = new();
        private readonly List<BinaryExpressionSyntax> isChecks = new();
        private readonly List<IsPatternExpressionSyntax> isPatterns = new();
        private readonly List<CasePatternSwitchLabelSyntax> caseLabels = new();

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

        internal static bool TryGetCommonBase(SyntaxNode node, IParameterSymbol symbol, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ITypeSymbol? sourceType)
        {
            sourceType = null;
            using var walker = Borrow(node);
            foreach (var cast in walker.casts)
            {
                if (IsFor(cast.Expression, symbol, semanticModel, cancellationToken) &&
                    !TryGetCommonBase(sourceType, cast.Type, out sourceType))
                {
                    return false;
                }
            }

            foreach (var cast in walker.asCasts)
            {
                if (IsFor(cast.Left, symbol, semanticModel, cancellationToken) &&
                    !TryGetCommonBase(sourceType, cast.Right as TypeSyntax, out sourceType))
                {
                    return false;
                }
            }

            foreach (var isCheck in walker.isChecks)
            {
                if (IsFor(isCheck.Left, symbol, semanticModel, cancellationToken) &&
                    !TryGetCommonBase(sourceType, isCheck.Right as TypeSyntax, out sourceType))
                {
                    return false;
                }
            }

            foreach (var isPattern in walker.isPatterns)
            {
                if (isPattern is { Expression: { } expression, Pattern: DeclarationPatternSyntax { Type: { } type } } &&
                    IsFor(expression, symbol, semanticModel, cancellationToken) &&
                    !TryGetCommonBase(sourceType, type, out sourceType))
                {
                    return false;
                }
            }

            foreach (var label in walker.caseLabels)
            {
                if (label is { Pattern: DeclarationPatternSyntax { Type: { } type }, Parent: SwitchSectionSyntax { Parent: SwitchStatementSyntax { Expression: { } expression } } } &&
                    IsFor(expression, symbol, semanticModel, cancellationToken) &&
                    !TryGetCommonBase(sourceType, type, out sourceType))
                {
                    return false;
                }
            }

            // If we couldn't "guess" a source type we take parameters type
            if (sourceType is null)
            {
                sourceType = symbol.Type;
            }

            return true;

            bool IsFor(ExpressionSyntax e, ISymbol s, SemanticModel sm, CancellationToken ct)
            {
                if (e is IdentifierNameSyntax idn &&
                    idn.Identifier.ValueText != symbol.Name)
                {
                    return false;
                }

                return SymbolComparer.Equal(sm.GetSymbolSafe(e, ct), s);
            }

            bool TryGetCommonBase(ITypeSymbol? t1, TypeSyntax? ts, out ITypeSymbol? result)
            {
                result = null!;
                if (ts is null)
                {
                    return false;
                }

                var t2 = semanticModel.GetTypeInfoSafe(ts, cancellationToken).Type;
                if (t2 is null)
                {
                    return false;
                }

                if (TypeSymbolComparer.Equal(t1, t2))
                {
                    result = t1;
                    return true;
                }

                if (t1 is null ||
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

                using var set = PooledSet<ITypeSymbol>.Borrow();
                set.UnionWith(t1.RecursiveBaseTypes());
                set.IntersectWith(t2.RecursiveBaseTypes());
                return set.TryFirst(x => x is INamedTypeSymbol { IsGenericType: true }, out result) ||
                       set.TryFirst(out result);
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
