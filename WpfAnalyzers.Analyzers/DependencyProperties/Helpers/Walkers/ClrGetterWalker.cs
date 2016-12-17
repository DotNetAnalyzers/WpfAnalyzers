namespace WpfAnalyzers.DependencyProperties
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class ClrGetterWalker : CSharpSyntaxWalker
    {
        private static readonly Pool<ClrGetterWalker> Cache = new Pool<ClrGetterWalker>(
            () => new ClrGetterWalker(),
            x =>
                {
                    x.semanticModel = null;
                    x.cancellationToken = CancellationToken.None;
                    x.HasError = false;
                    x.GetValue = null;
                });

        private SemanticModel semanticModel;
        private CancellationToken cancellationToken;

        private ClrGetterWalker()
        {
        }

        public bool IsSuccess => !this.HasError && this.GetValue != null;

        public bool HasError { get; private set; }

        public InvocationExpressionSyntax GetValue { get; private set; }

        public ArgumentSyntax Property => this.GetValue?.ArgumentList.Arguments[0];

        public static Pool<ClrGetterWalker>.Pooled Create(SemanticModel semanticModel, CancellationToken cancellationToken, SyntaxNode getter)
        {
            var pooled = Cache.GetOrCreate();
            pooled.Item.semanticModel = semanticModel;
            pooled.Item.cancellationToken = cancellationToken;
            pooled.Item.Visit(getter);
            return pooled;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax invocation)
        {
            ArgumentSyntax property;
            IFieldSymbol getField;
            if (DependencyObject.TryGetGetValueArgument(invocation, this.semanticModel, this.cancellationToken, out property, out getField))
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
    }
}