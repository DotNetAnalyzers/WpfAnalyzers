namespace WpfAnalyzers
{
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class ClrSetterWalker : PooledWalker<ClrSetterWalker>
    {
        private SemanticModel semanticModel = null!;
        private CancellationToken cancellationToken;

        private ClrSetterWalker()
        {
        }

        internal bool IsSuccess => !this.HasError && this.Arguments is { };

        internal bool HasError { get; private set; }

        internal InvocationExpressionSyntax? SetValue { get; private set; }

        internal InvocationExpressionSyntax? SetCurrentValue { get; private set; }

        internal ArgumentListSyntax? Arguments => this.SetValue?.ArgumentList ?? this.SetCurrentValue?.ArgumentList;

        internal ArgumentSyntax? Property => this.Arguments?.Arguments.FirstOrDefault();

        public override void VisitInvocationExpression(InvocationExpressionSyntax invocation)
        {
            if (DependencyObject.SetValue.Match(invocation, this.semanticModel, this.cancellationToken) is { })
            {
                if (this.SetValue is { } || this.SetCurrentValue is { })
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
                if (this.SetValue is { } || this.SetCurrentValue is { })
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

        public override void Visit(SyntaxNode? node)
        {
            if (this.HasError)
            {
                return;
            }

            base.Visit(node);
        }

        internal static ClrSetterWalker Borrow(SemanticModel semanticModel, SyntaxNode setter, CancellationToken cancellationToken)
        {
            var walker = Borrow(() => new ClrSetterWalker());
            walker.semanticModel = semanticModel;
            walker.cancellationToken = cancellationToken;
            walker.Visit(setter);
            return walker;
        }

        protected override void Clear()
        {
            this.semanticModel = null!;
            this.cancellationToken = CancellationToken.None;
            this.HasError = false;
            this.SetValue = null!;
            this.SetCurrentValue = null!;
        }
    }
}
