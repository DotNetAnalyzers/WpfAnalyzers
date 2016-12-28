namespace WpfAnalyzers.PropertyChanged.Helpers
{
    using System.Collections.Generic;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class IfStatementWalker : CSharpSyntaxWalker
    {
        private static readonly Pool<IfStatementWalker> Cache = new Pool<IfStatementWalker>(
            () => new IfStatementWalker(),
            x => x.ifStatements.Clear());

        private readonly List<IfStatementSyntax> ifStatements = new List<IfStatementSyntax>();

        private IfStatementWalker()
        {
        }

        public IReadOnlyList<IfStatementSyntax> IfStatements => this.ifStatements;

        public static Pool<IfStatementWalker>.Pooled Create(SyntaxNode node)
        {
            var pooled = Cache.GetOrCreate();
            pooled.Item.Visit(node);
            return pooled;
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            this.ifStatements.Add(node);
            base.VisitIfStatement(node);
        }
    }
}