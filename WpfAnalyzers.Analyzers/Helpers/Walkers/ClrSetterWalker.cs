namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class ClrSetterWalker : CSharpSyntaxWalker
    {
        private static readonly Pool<ClrSetterWalker> Cache = new Pool<ClrSetterWalker>(
            () => new ClrSetterWalker(),
            x =>
            {
                x.semanticModel = null;
                x.cancellationToken = CancellationToken.None;
                x.HasError = false;
                x.SetValue = null;
                x.SetCurrentValue = null;
            });

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

        public ArgumentSyntax Value => this.Arguments?.Arguments[1];

        public static Pool<ClrSetterWalker>.Pooled Create(SemanticModel semanticModel, CancellationToken cancellationToken, SyntaxNode setter)
        {
            var pooled = Cache.GetOrCreate();
            pooled.Item.semanticModel = semanticModel;
            pooled.Item.cancellationToken = cancellationToken;
            pooled.Item.Visit(setter);
            return pooled;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax invocation)
        {
            if (DependencyObject.TryGetSetValueArguments(invocation, this.semanticModel, this.cancellationToken, out ArgumentListSyntax arguments))
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

            if (DependencyObject.TryGetSetCurrentValueArguments(invocation, this.semanticModel, this.cancellationToken, out arguments))
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
    }
}