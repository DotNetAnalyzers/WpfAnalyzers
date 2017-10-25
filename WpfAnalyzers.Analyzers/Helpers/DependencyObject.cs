#pragma warning disable 1573
namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class DependencyObject
    {
        internal static bool TryGetSetValueCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return TryGetCall(
                invocation,
                KnownSymbol.DependencyObject.SetValue,
                2,
                semanticModel,
                cancellationToken,
                out method);
        }

        internal static bool TryGetSetCurrentValueCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return TryGetCall(
                invocation,
                KnownSymbol.DependencyObject.SetCurrentValue,
                2,
                semanticModel,
                cancellationToken,
                out method);
        }

        internal static bool TryGetGetValueCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return TryGetCall(
                invocation,
                KnownSymbol.DependencyObject.GetValue,
                1,
                semanticModel,
                cancellationToken,
                out method);
        }

        private static bool TryGetCall(InvocationExpressionSyntax invocation, QualifiedMethod qualifiedMethod, int expectedArgs, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            method = null;
            if (invocation == null ||
                invocation.ArgumentList == null ||
                invocation.ArgumentList.Arguments.Count != expectedArgs)
            {
                return false;
            }

            if (invocation.TryGetInvokedMethodName(out var name) &&
                name != qualifiedMethod.Name)
            {
                return false;
            }

            method = semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol;
            return method == qualifiedMethod;
        }
    }
}