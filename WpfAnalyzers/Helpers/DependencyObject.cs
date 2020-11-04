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
