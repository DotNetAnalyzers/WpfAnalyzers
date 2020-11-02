namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class ClrGetterWalker : PooledWalker<ClrGetterWalker>
    {
        private SemanticModel semanticModel = null!;
        private CancellationToken cancellationToken;

        private ClrGetterWalker()
        {
        }

        internal bool IsSuccess => !this.HasError && this.GetValue is { };

        internal bool HasError { get; private set; }

        internal InvocationExpressionSyntax? GetValue { get; private set; }

        internal ArgumentSyntax? Property => this.GetValue?.ArgumentList?.Arguments.FirstOrDefault();

        public override void VisitInvocationExpression(InvocationExpressionSyntax invocation)
        {
            if (DependencyObject.TryGetGetValueCall(invocation, this.semanticModel, this.cancellationToken, out _))
            {
                if (this.Property is { })
                {
                    this.HasError = true;
                    this.GetValue = null;
                }
                else
                {
                    this.GetValue = invocation;
                }
            }

            base.VisitInvocationExpression(invocation);
        }

        public override void Visit(SyntaxNode? node)
        {
            if (this.HasError)
            {
                return;
            }

            base.Visit(node);
        }

        internal static ClrGetterWalker Borrow(SemanticModel semanticModel, SyntaxNode getter, CancellationToken cancellationToken)
        {
            var walker = Borrow(() => new ClrGetterWalker());
            walker.semanticModel = semanticModel;
            walker.cancellationToken = cancellationToken;
            walker.Visit(getter);
            return walker;
        }

        protected override void Clear()
        {
            this.semanticModel = null!;
            this.cancellationToken = CancellationToken.None;
            this.HasError = false;
            this.GetValue = null!;
        }
    }
}
