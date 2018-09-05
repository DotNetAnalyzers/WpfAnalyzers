namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class InvocationWalker : PooledWalker<InvocationWalker>
    {
        private readonly List<IdentifierNameSyntax> identifierNames = new List<IdentifierNameSyntax>();
        private IMethodSymbol method;
        private SemanticModel semanticModel;
        private CancellationToken cancellationToken;

        private InvocationWalker()
        {
        }

        internal IReadOnlyList<IdentifierNameSyntax> IdentifierNames => this.identifierNames;

        /// <inheritdoc />
        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (node.Identifier.ValueText == this.method.MetadataName &&
                this.semanticModel.TryGetSymbol(node, this.cancellationToken, out IMethodSymbol candidate) &&
                candidate.Equals(this.method))
            {
                this.identifierNames.Add(node);
            }

            base.VisitIdentifierName(node);
        }

        internal static InvocationWalker Borrow(IMethodSymbol method, SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var walker = Borrow(() => new InvocationWalker());
            walker.method = method;
            walker.semanticModel = semanticModel;
            walker.cancellationToken = cancellationToken;
            walker.Visit(node);
            return walker;
        }

        protected override void Clear()
        {
            this.method = null;
            this.semanticModel = null;
            this.cancellationToken = CancellationToken.None;
            this.identifierNames.Clear();
        }
    }
}
