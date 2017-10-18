namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class IfStatementWalker : PooledWalker<IfStatementWalker>
    {
        private readonly List<IfStatementSyntax> ifStatements = new List<IfStatementSyntax>();

        private IfStatementWalker()
        {
        }

        public IReadOnlyList<IfStatementSyntax> IfStatements => this.ifStatements;

        public static IfStatementWalker Borrow(SyntaxNode node) => BorrowAndVisit(node, () => new IfStatementWalker());

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            this.ifStatements.Add(node);
            base.VisitIfStatement(node);
        }

        protected override void Clear()
        {
            this.ifStatements.Clear();
        }
    }
}