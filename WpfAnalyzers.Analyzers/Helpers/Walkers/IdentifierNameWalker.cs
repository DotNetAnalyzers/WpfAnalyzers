namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class IdentifierNameWalker : PooledWalker<IdentifierNameWalker>
    {
        private readonly List<IdentifierNameSyntax> identifierNames = new List<IdentifierNameSyntax>();

        private IdentifierNameWalker()
        {
        }

        internal IReadOnlyList<IdentifierNameSyntax> IdentifierNames => this.identifierNames;

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            this.identifierNames.Add(node);
            base.VisitIdentifierName(node);
        }

        internal static IdentifierNameWalker Borrow(SyntaxNode node) => BorrowAndVisit(node, () => new IdentifierNameWalker());

        internal static IdentifierNameSyntax First(SyntaxNode node)
        {
            using (var walker = Borrow(node))
            {
                return walker.identifierNames[0];
            }
        }

        protected override void Clear()
        {
            this.identifierNames.Clear();
        }
    }
}
