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

        public InvocationExpressionSyntax SetValue { get; private set; }

        public InvocationExpressionSyntax SetCurrentValue { get; private set; }

        public ArgumentListSyntax Arguments => this.SetValue?.ArgumentList ?? this.SetCurrentValue?.ArgumentList;

        public ArgumentSyntax Property => this.Arguments?.Arguments[0];

        public ArgumentSyntax Value => this.Arguments?.Arguments[1];

        public static ClrSetterWalker Create(SemanticModel semanticModel, CancellationToken cancellationToken, SyntaxNode setter)
        {
            ClrSetterWalker walker;
            if (!Cache.TryDequeue(out walker))
            {
                walker = new ClrSetterWalker();
            }

            walker.HasError = false;
            walker.SetValue = null;
            walker.SetCurrentValue = null;
            walker.semanticModel = semanticModel;
            walker.cancellationToken = cancellationToken;
            walker.Visit(setter);
            return walker;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax invocation)
        {
            ArgumentListSyntax arguments;
            if (DependencyObject.TryGetSetValueArguments(invocation, this.semanticModel, this.cancellationToken, out arguments))
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

        public void Dispose()
        {
            this.HasError = false;
            this.SetValue = null;
            this.SetCurrentValue = null;
            this.semanticModel = null;
            this.cancellationToken = CancellationToken.None;
            Cache.Enqueue(this);
        }
    }
}