namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class InvocationWalker : CSharpSyntaxWalker
    {
        private static readonly Pool<InvocationWalker> Pool = new Pool<InvocationWalker>(
                                                                  () => new InvocationWalker(),
                                                                  x => x.invocations.Clear());

        private readonly List<InvocationExpressionSyntax> invocations = new List<InvocationExpressionSyntax>();

        private InvocationWalker()
        {
        }

        public IReadOnlyList<InvocationExpressionSyntax> Invocations => this.invocations;

        public static Pool<InvocationWalker>.Pooled Create(SyntaxNode node)
        {
            var pooled = Pool.GetOrCreate();
            if (node != null)
            {
                pooled.Item.Visit(node);
            }

            return pooled;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            this.invocations.Add(node);
            base.VisitInvocationExpression(node);
        }
    }
}