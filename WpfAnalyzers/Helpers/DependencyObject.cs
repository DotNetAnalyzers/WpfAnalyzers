#pragma warning disable 1573
namespace WpfAnalyzers
{
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class DependencyObject
    {
        internal readonly struct GetValue
        {
            internal readonly InvocationExpressionSyntax Invocation;
            internal readonly IMethodSymbol Target;

            private GetValue(InvocationExpressionSyntax invocation, IMethodSymbol target)
            {
                this.Invocation = invocation;
                this.Target = target;
            }

            internal ArgumentSyntax PropertyArgument => this.Invocation.ArgumentList.Arguments[0];

            internal static GetValue? Find(MethodOrAccessor node, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                return node switch
                {
                    { ExpressionBody: { Expression: InvocationExpressionSyntax invocation } }
                        => Match(invocation, semanticModel, cancellationToken),
                    { ExpressionBody: { Expression: CastExpressionSyntax { Expression: InvocationExpressionSyntax invocation } } }
                        => Match(invocation, semanticModel, cancellationToken),
                    { ExpressionBody: { } expressionBody } => Walk(expressionBody),
                    { Body: { Statements: { } statements } }
                        when statements.Last() is ReturnStatementSyntax { Expression: InvocationExpressionSyntax invocation }
                        => Match(invocation, semanticModel, cancellationToken),
                    { Body: { Statements: { } statements } }
                        when statements.Last() is ReturnStatementSyntax { Expression: CastExpressionSyntax { Expression: InvocationExpressionSyntax invocation } }
                        => Match(invocation, semanticModel, cancellationToken),
                    { Body: { } body } => Walk(body),
                    _ => null,
                };

                GetValue? Walk(SyntaxNode body)
                {
                    using var getterWalker = Walker.Borrow(semanticModel, body, cancellationToken);
                    if (getterWalker is { HasError: false, Result: { } getValue })
                    {
                        return getValue;
                    }

                    return null;
                }
            }

            internal static GetValue? Match(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (invocation is { ArgumentList: { Arguments: { Count: 1 } arguments } } &&
                    arguments[0] is { Expression: { } } &&
                    semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyObject.GetValue, cancellationToken, out var method))
                {
                    return new GetValue(invocation, method);
                }

                return null;
            }

            private class Walker : PooledWalker<Walker>
            {
                internal bool HasError;
                internal GetValue? Result;

                private SemanticModel semanticModel = null!;
                private CancellationToken cancellationToken;

                private Walker()
                {
                }

                public override void VisitInvocationExpression(InvocationExpressionSyntax invocation)
                {
                    if (Match(invocation, this.semanticModel, this.cancellationToken) is { } getValue)
                    {
                        if (this.Result is { })
                        {
                            this.HasError = true;
                            this.Result = null;
                        }
                        else
                        {
                            this.Result = getValue;
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

                internal static Walker Borrow(SemanticModel semanticModel, SyntaxNode getter, CancellationToken cancellationToken)
                {
                    var walker = Borrow(() => new Walker());
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
                    this.Result = null;
                }
            }
        }

        internal readonly struct SetValue
        {
            internal readonly InvocationExpressionSyntax Invocation;
            internal readonly IMethodSymbol Target;

            private SetValue(InvocationExpressionSyntax invocation, IMethodSymbol target)
            {
                this.Invocation = invocation;
                this.Target = target;
            }

            internal static SetValue? Match(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (invocation is { ArgumentList: { Arguments: { Count: 2 } arguments } } &&
                    arguments[0] is { Expression: { } } &&
                    arguments[1] is { Expression: { } } &&
                    semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyObject.SetValue, cancellationToken, out var method))
                {
                    return new SetValue(invocation, method);
                }

                return null;
            }
        }

        internal readonly struct SetCurrentValue
        {
            internal readonly InvocationExpressionSyntax Invocation;
            internal readonly IMethodSymbol Target;

            private SetCurrentValue(InvocationExpressionSyntax invocation, IMethodSymbol target)
            {
                this.Invocation = invocation;
                this.Target = target;
            }

            internal static SetCurrentValue? Match(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (invocation is { ArgumentList: { Arguments: { Count: 2 } arguments } } &&
                    arguments[0] is { Expression: { } } &&
                    arguments[1] is { Expression: { } } &&
                    semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyObject.SetCurrentValue, cancellationToken, out var method))
                {
                    return new SetCurrentValue(invocation, method);
                }

                return null;
            }
        }
    }
}
