namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class ClrSetterWalker : PooledWalker<ClrSetterWalker>
    {
        private SemanticModel semanticModel;
        private CancellationToken cancellationToken;

        private ClrSetterWalker()
        {
        }

        public bool IsSuccess => !this.HasError && this.Arguments != null;

        public bool HasError { get; private set; }

        public InvocationExpressionSyntax SetValue { get; private set; }

        public InvocationExpressionSyntax SetCurrentValue { get; private set; }

        public ArgumentListSyntax Arguments => this.SetValue?.ArgumentList ?? this.SetCurrentValue?.ArgumentList;

        public ArgumentSyntax Property => this.Arguments?.Arguments[0];

        public static ClrSetterWalker Borrow(SemanticModel semanticModel, CancellationToken cancellationToken, SyntaxNode setter)
        {
            var walker = Borrow(() => new ClrSetterWalker());
            walker.semanticModel = semanticModel;
            walker.cancellationToken = cancellationToken;
            walker.Visit(setter);
            return walker;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax invocation)
        {
            if (DependencyObject.TryGetSetValueCall(invocation, this.semanticModel, this.cancellationToken, out _))
            {
                if (this.SetValue != null || this.SetCurrentValue != null)
                {
                    this.HasError = true;
                    this.SetValue = null;
                    this.SetCurrentValue = null;
                }
                else
                {
                    this.SetValue = invocation;
                }
            }

            if (DependencyObject.TryGetSetCurrentValueCall(invocation, this.semanticModel, this.cancellationToken, out _))
            {
                if (this.SetValue != null || this.SetCurrentValue != null)
                {
                    this.HasError = true;
                    this.SetValue = null;
                    this.SetCurrentValue = null;
                }
                else
                {
                    this.SetCurrentValue = invocation;
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
            this.SetValue = null;
            this.SetCurrentValue = null;
        }
    }
}
