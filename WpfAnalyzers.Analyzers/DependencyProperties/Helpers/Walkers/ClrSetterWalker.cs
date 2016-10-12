namespace WpfAnalyzers.DependencyProperties
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class ClrSetterWalker : CSharpSyntaxWalker, IDisposable
    {
        private static readonly ConcurrentQueue<ClrSetterWalker> Cache = new ConcurrentQueue<ClrSetterWalker>();

        private SemanticModel semanticModel;
        private CancellationToken cancellationToken;

        private ClrSetterWalker()
        {
        }

        public bool IsSuccess => !this.HasError && this.Arguments != null;

        public bool HasError { get; private set; }

        public ArgumentListSyntax Arguments { get; private set; }

        public ArgumentSyntax Property => this.Arguments?.Arguments[0];

        public ArgumentSyntax Value => this.Arguments?.Arguments[1];

        public static ClrSetterWalker Create(SemanticModel semanticModel, CancellationToken cancellationToken, AccessorDeclarationSyntax setter)
        {
            ClrSetterWalker walker;
            if (!Cache.TryDequeue(out walker))
            {
                walker = new ClrSetterWalker();
            }

            walker.HasError = false;
            walker.Arguments = null;
            walker.semanticModel = semanticModel;
            walker.cancellationToken = cancellationToken;
            walker.Visit(setter);
            return walker;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            ArgumentListSyntax arguments;
            if (node.TryGetSetValueArguments(this.semanticModel, this.cancellationToken, out arguments))
            {
                if (this.Property != null)
                {
                    this.HasError = true;
                    this.Arguments = null;
                }
                else
                {
                    this.Arguments = arguments;
                }
            }

            base.VisitInvocationExpression(node);
        }

        public void Dispose()
        {
            this.HasError = false;
            this.Arguments = null;
            this.semanticModel = null;
            this.cancellationToken = CancellationToken.None;
            Cache.Enqueue(this);
        }
    }
}