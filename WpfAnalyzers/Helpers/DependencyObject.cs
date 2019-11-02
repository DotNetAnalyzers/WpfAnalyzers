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
        internal static bool TryGetSetValueCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out IMethodSymbol? method)
        {
            return TryGetCall(
                invocation,
                KnownSymbols.DependencyObject.SetValue,
                2,
                semanticModel,
                cancellationToken,
                out method);
        }

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
    }
}
