namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class ClrGetterWalker : PooledWalker<ClrGetterWalker>
    {
        private SemanticModel semanticModel;
        private CancellationToken cancellationToken;

        private ClrGetterWalker()
        {
        }

        public bool IsSuccess => !this.HasError && this.GetValue != null;

        public bool HasError { get; private set; }

        public InvocationExpressionSyntax GetValue { get; private set; }

        public ArgumentSyntax Property => this.GetValue?.ArgumentList.Arguments[0];

        public static ClrGetterWalker Borrow(SemanticModel semanticModel, CancellationToken cancellationToken, SyntaxNode getter)
        {
            var walker = Borrow(() => new ClrGetterWalker());
            walker.semanticModel = semanticModel;
            walker.cancellationToken = cancellationToken;
            walker.Visit(getter);
            return walker;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax invocation)
        {
            if (DependencyObject.TryGetGetValueCall(invocation, this.semanticModel, this.cancellationToken, out _))
            {
                if (this.Property != null)
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

        public override void Visit(SyntaxNode node)
        {
            if (this.HasError)
            {
                return;
            }

            base.Visit(node);
        }

        protected override void Clear()
        {
            this.semanticModel = null;
            this.cancellationToken = CancellationToken.None;
            this.HasError = false;
            this.GetValue = null;
        }
    }
}
