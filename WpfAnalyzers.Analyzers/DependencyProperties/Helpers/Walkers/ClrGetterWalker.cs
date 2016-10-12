namespace WpfAnalyzers.DependencyProperties
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class ClrGetterWalker : CSharpSyntaxWalker, IDisposable
    {
        private static readonly ConcurrentQueue<ClrGetterWalker> Cache = new ConcurrentQueue<ClrGetterWalker>();

        private SemanticModel semanticModel;
        private CancellationToken cancellationToken;

        private ClrGetterWalker()
        {
        }

        public bool IsSuccess => !this.HasError && this.GetValue != null;

        public bool HasError { get; private set; }

        public InvocationExpressionSyntax GetValue { get; private set; }

        public ArgumentSyntax Property => this.GetValue?.ArgumentList.Arguments[0];

        public static ClrGetterWalker Create(SemanticModel semanticModel, CancellationToken cancellationToken, AccessorDeclarationSyntax getter)
        {
            ClrGetterWalker walker;
            if (!Cache.TryDequeue(out walker))
            {
                walker = new ClrGetterWalker();
            }

            walker.HasError = false;
            walker.GetValue = null;
            walker.semanticModel = semanticModel;
            walker.cancellationToken = cancellationToken;
            walker.Visit(getter);
            return walker;
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

        public void Dispose()
        {
            this.HasError = false;
            this.GetValue = null;
            this.semanticModel = null;
            this.cancellationToken = CancellationToken.None;
            Cache.Enqueue(this);
        }
    }
}