namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class ReturnExpressionsWalker : PooledWalker<ReturnExpressionsWalker>
    {
        private readonly List<ExpressionSyntax> returnValues = new List<ExpressionSyntax>();

        private ReturnExpressionsWalker()
        {
        }

        public IReadOnlyList<ExpressionSyntax> ReturnValues => this.returnValues;

        public static ReturnExpressionsWalker Create(SyntaxNode node) => BorrowAndVisit(node, () => new ReturnExpressionsWalker());

        public override void VisitReturnStatement(ReturnStatementSyntax node)
        {
            this.returnValues.Add(node.Expression);
            base.VisitReturnStatement(node);
        }

        public override void VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
        {
            this.returnValues.Add(node.Expression);
            base.VisitArrowExpressionClause(node);
        }

        protected override void Clear()
        {
            this.returnValues.Clear();
        }
    }
}