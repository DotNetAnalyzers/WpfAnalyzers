namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class ReturnExpressionsWalker : CSharpSyntaxWalker
    {
        private static readonly Pool<ReturnExpressionsWalker> Pool = new Pool<ReturnExpressionsWalker>(
            () => new ReturnExpressionsWalker(),
            x => x.returnValues.Clear());

        private readonly List<ExpressionSyntax> returnValues = new List<ExpressionSyntax>();

        private ReturnExpressionsWalker()
        {
        }

        public IReadOnlyList<ExpressionSyntax> ReturnValues => this.returnValues;

        public static Pool<ReturnExpressionsWalker>.Pooled Create(SyntaxNode node)
        {
            var pooled = Pool.GetOrCreate();
            pooled.Item.Visit(node);
            return pooled;
        }

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
    }
}