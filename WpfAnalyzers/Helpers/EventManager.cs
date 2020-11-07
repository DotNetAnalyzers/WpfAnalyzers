namespace WpfAnalyzers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class EventManager
    {
        internal static bool? IsMatch(string handlerName, string eventName)
        {
            if (!eventName.EndsWith("Event", StringComparison.Ordinal) ||
                !handlerName.StartsWith("On", StringComparison.Ordinal) ||
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

        internal static bool TryGetExpectedCallbackName(string eventName, [NotNullWhen(true)] out string? expectedName)
        {
            if (eventName.EndsWith("Event", StringComparison.Ordinal))
            {
                expectedName = "On" + eventName.Remove(eventName.Length - "Event".Length);
                return true;
            }

            expectedName = null;
            return false;
        }

        internal readonly struct RegisterClassHandler
        {
            internal readonly InvocationExpressionSyntax Invocation;
            internal readonly IMethodSymbol Target;

            internal RegisterClassHandler(InvocationExpressionSyntax invocation, IMethodSymbol target)
            {
                this.Invocation = invocation;
                this.Target = target;
            }

            internal ArgumentSyntax? EventArgument
            {
                get
                {
                    if (this.Invocation.TryGetArgumentAtIndex(1, out var argument))
                    {
                        return argument;
                    }

                    return null;
                }
            }

            internal static RegisterClassHandler? Match(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                if (invocation is { ArgumentList: { Arguments: { } arguments } } &&
                    (arguments.Count == 3 || arguments.Count == 4) &&
                    semanticModel.TryGetSymbol(invocation, KnownSymbols.EventManager.RegisterClassHandler, cancellationToken, out var method))
                {
                    return new RegisterClassHandler(invocation, method);
                }

                return null;
            }
        }
    }
}
