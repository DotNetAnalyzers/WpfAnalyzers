namespace WpfAnalyzers
{
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class ClrGetterWalker : PooledWalker<ClrGetterWalker>
    {
        private SemanticModel semanticModel = null!;
        private CancellationToken cancellationToken;

        private ClrGetterWalker()
        {
        }

        internal bool IsSuccess => !this.HasError && this.GetValue is { };

        internal bool HasError { get; private set; }

        internal DependencyObject.GetValue? GetValue { get; private set; }

        internal ArgumentSyntax? Property => this.GetValue?.Invocation.ArgumentList?.Arguments.FirstOrDefault();

        public override void VisitInvocationExpression(InvocationExpressionSyntax invocation)
        {
            if (DependencyObject.GetValue.Match(invocation, this.semanticModel, this.cancellationToken) is { } getValue)
            {
                if (this.Property is { })
                {
                    this.HasError = true;
                    this.GetValue = null;
                }
                else
                {
                    this.GetValue = getValue;
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

        internal static DependencyObject.GetValue? FindGetValue(MethodOrAccessor node, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return node switch
            {
                { ExpressionBody: { Expression: InvocationExpressionSyntax invocation } }
                    => DependencyObject.GetValue.Match(invocation, semanticModel, cancellationToken),
                { ExpressionBody: { Expression: CastExpressionSyntax { Expression: InvocationExpressionSyntax invocation } } }
                    => DependencyObject.GetValue.Match(invocation, semanticModel, cancellationToken),
                { ExpressionBody: { } expressionBody } => Walk(expressionBody),
                { Body: { Statements: { } statements } }
                    when statements.Last() is ReturnStatementSyntax { Expression: InvocationExpressionSyntax invocation }
                    => DependencyObject.GetValue.Match(invocation, semanticModel, cancellationToken),
                { Body: { Statements: { } statements } }
                    when statements.Last() is ReturnStatementSyntax { Expression: CastExpressionSyntax { Expression: InvocationExpressionSyntax invocation } }
                    => DependencyObject.GetValue.Match(invocation, semanticModel, cancellationToken),
                { Body: { } body } => Walk(body),
                _ => null,
            };

            DependencyObject.GetValue? Walk(SyntaxNode body)
            {
                using var getterWalker = Borrow(semanticModel, body, cancellationToken);
                if (getterWalker is { HasError: false, IsSuccess: true, GetValue: { } getValue })
                {
                    return getValue;
                }

                return null;
            }
        }

        internal static ClrGetterWalker Borrow(SemanticModel semanticModel, SyntaxNode getter, CancellationToken cancellationToken)
        {
            var walker = Borrow(() => new ClrGetterWalker());
            walker.semanticModel = semanticModel;
            walker.cancellationToken = cancellationToken;
            walker.Visit(getter);
            return walker;
        }

        protected override void Clear()
        {
            this.semanticModel = null!;
            this.cancellationToken = CancellationToken.None;
            this.HasError = false;
            this.GetValue = null!;
        }
    }
}
