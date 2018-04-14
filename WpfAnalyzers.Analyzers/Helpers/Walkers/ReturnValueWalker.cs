namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class ReturnValueWalker : PooledWalker<ReturnValueWalker>
    {
        private readonly List<ExpressionSyntax> returnValues = new List<ExpressionSyntax>();

        private ReturnValueWalker()
        {
        }

        public IReadOnlyList<ExpressionSyntax> ReturnValues => this.returnValues;

        public static ReturnValueWalker Borrow(SyntaxNode node) => BorrowAndVisit(node, () => new ReturnValueWalker());

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
