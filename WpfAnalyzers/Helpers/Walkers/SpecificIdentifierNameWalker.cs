namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class SpecificIdentifierNameWalker : PooledWalker<SpecificIdentifierNameWalker>
    {
        private readonly List<IdentifierNameSyntax> identifierNames = new List<IdentifierNameSyntax>();
        private string name = null!;

        private SpecificIdentifierNameWalker()
        {
        }

        internal IReadOnlyList<IdentifierNameSyntax> IdentifierNames => this.identifierNames;

        /// <inheritdoc />
        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (node.Identifier.ValueText == this.name)
            {
                this.identifierNames.Add(node);
            }

            base.VisitIdentifierName(node);
        }

        internal static SpecificIdentifierNameWalker Borrow(SyntaxNode node, string name)
        {
            var walker = Borrow(() => new SpecificIdentifierNameWalker());
            walker.name = name;
            walker.Visit(node);
            return walker;
        }

        protected override void Clear()
        {
            this.name = null!;
            this.identifierNames.Clear();
        }
    }
}
