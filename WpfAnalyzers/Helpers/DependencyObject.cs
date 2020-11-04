#pragma warning disable 1573
namespace WpfAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class DependencyObject
    {
        internal static bool TryGetSetCurrentValueCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out IMethodSymbol? method)
        {
            return TryGetCall(
                invocation,
                KnownSymbols.DependencyObject.SetCurrentValue,
                2,
                semanticModel,
                cancellationToken,
                out method);
        }

        internal static bool TryGetGetValueCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out IMethodSymbol? method)
        {
            return TryGetCall(
                invocation,
                KnownSymbols.DependencyObject.GetValue,
                1,
                semanticModel,
                cancellationToken,
                out method);
        }

        private static bool TryGetCall(InvocationExpressionSyntax invocation, QualifiedMethod qualifiedMethod, int expectedArgs, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out IMethodSymbol? method)
        {
            method = null;
            return invocation.ArgumentList is { } argumentList &&
                   argumentList.Arguments.Count == expectedArgs &&
                   semanticModel.TryGetSymbol(invocation, qualifiedMethod, cancellationToken, out method);
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
                if (invocation is { ArgumentList: { Arguments: { Count: 2 } } } &&
                    semanticModel.TryGetSymbol(invocation, KnownSymbols.DependencyObject.SetValue, cancellationToken, out var method))
                {
                    return new SetValue(invocation, method);
                }

                return null;
            }
        }
    }
}
