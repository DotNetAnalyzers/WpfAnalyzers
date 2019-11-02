namespace WpfAnalyzers
{
    using System;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class EventManager
    {
        internal static bool TryGetRegisterClassHandlerCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return TryGetCall(
                invocation,
                KnownSymbols.EventManager.RegisterClassHandler,
                semanticModel,
                cancellationToken,
                out method);
        }

        internal static bool? IsMatch(string handlerName, string eventName)
        {
            if (handlerName == null ||
                eventName == null ||
                !eventName.EndsWith("Event", StringComparison.Ordinal))
            {
                return false;
            }

            if (!handlerName.StartsWith("On", StringComparison.Ordinal) ||
                handlerName.Length != eventName.Length - 3)
            {
                return false;
            }

            for (var i = 0; i < eventName.Length - 5; i++)
            {
                if (handlerName[i + 2] != eventName[i])
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool TryGetExpectedCallbackName(string eventName, out string expectedName)
        {
            if (eventName.EndsWith("Event", StringComparison.Ordinal))
            {
                expectedName = "On" + eventName.Remove(eventName.Length - "Event".Length);
                return true;
            }

            expectedName = null;
            return false;
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

            return semanticModel.TryGetSymbol(invocation, qualifiedMethod, cancellationToken, out method);
        }
    }
}
