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
        private bool hasError;

        private ClrGetterWalker()
        {
        }

        public bool IsSuccess => !this.hasError && this.Property != null;

        public ArgumentSyntax Property { get; private set; }

        public static ClrGetterWalker Create(SemanticModel semanticModel, CancellationToken cancellationToken, AccessorDeclarationSyntax getter)
        {
            ClrGetterWalker walker;
            if (!Cache.TryDequeue(out walker))
            {
                walker = new ClrGetterWalker();
            }

            walker.hasError = false;
            walker.Property = null;
            walker.semanticModel = semanticModel;
            walker.cancellationToken = cancellationToken;
            walker.Visit(getter);
            return walker;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            ArgumentSyntax property;
            if (node.TryGetGetValueArgument(this.semanticModel, this.cancellationToken, out property))
            {
                if (this.Property != null)
                {
                    this.hasError = true;
                    this.Property = null;
                }
                else
                {
                    this.Property = property;
                }
            }

            base.VisitInvocationExpression(node);
        }

        public void Dispose()
        {
            this.hasError = false;
            this.Property = null;
            this.semanticModel = null;
            this.cancellationToken = CancellationToken.None;
            Cache.Enqueue(this);
        }
    }
}