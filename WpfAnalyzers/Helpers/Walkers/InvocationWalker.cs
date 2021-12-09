namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class InvocationWalker : PooledWalker<InvocationWalker>
    {
        private readonly List<IdentifierNameSyntax> identifierNames = new();
        private IMethodSymbol method = null!;
        private SemanticModel semanticModel = null!;
        private CancellationToken cancellationToken;

        private InvocationWalker()
        {
        }

        internal IReadOnlyList<IdentifierNameSyntax> IdentifierNames => this.identifierNames;

        /// <inheritdoc />
        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            // PERF: Avoid getting symbol for known cases.
            switch (node)
            {
                case { Parent: TypeSyntax _ }:
                case { Parent: BaseTypeSyntax _ }:
                case { Parent: ParameterSyntax _ }:
                case { Parent: VariableDeclarationSyntax _ }:
                case { Parent: MemberDeclarationSyntax _ }:
                    return;
            }

            if (node.Identifier.ValueText == this.method.MetadataName &&
                this.semanticModel.TryGetSymbol(node, this.cancellationToken, out IMethodSymbol? candidate) &&
                MethodSymbolComparer.Equal(candidate, this.method))
            {
                this.identifierNames.Add(node);
            }

            base.VisitIdentifierName(node);
        }

        internal static InvocationWalker InContainingClass(IMethodSymbol method, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var walker = Borrow(() => new InvocationWalker());
            walker.method = method;
            walker.semanticModel = semanticModel;
            walker.cancellationToken = cancellationToken;
            foreach (var reference in method.ContainingType.DeclaringSyntaxReferences)
            {
                walker.Visit(reference.GetSyntax(cancellationToken));
            }

            return walker;
        }

        protected override void Clear()
        {
            this.method = null!;
            this.semanticModel = null!;
            this.cancellationToken = CancellationToken.None;
            this.identifierNames.Clear();
        }
    }
}
