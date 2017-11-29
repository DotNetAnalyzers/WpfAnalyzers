namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class EventManager
    {
        internal static bool TryRegisterClassHandlerCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return TryGetCall(
                invocation,
                KnownSymbol.EventManager.RegisterClassHandler,
                semanticModel,
                cancellationToken,
                out method);
        }

        internal static bool? IsMatch(string handlerName, string eventName)
        {
            if (handlerName == null ||
                eventName == null ||
                !eventName.EndsWith("Event"))
            {
                return false;
            }

            if (!handlerName.StartsWith("On") ||
                handlerName.Length != eventName.Length - 3)
            {
                return false;
            }

            for (var i = 0; i < eventName.Length; i++)
            {
                if (handlerName[i + 2] != eventName[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryGetCall(InvocationExpressionSyntax invocation, QualifiedMethod qualifiedMethod, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            method = null;
            if (invocation?.ArgumentList == null ||
                invocation.ArgumentList.Arguments.Count < 3 ||
                invocation.ArgumentList.Arguments.Count > 4)
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