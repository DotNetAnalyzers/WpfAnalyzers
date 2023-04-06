namespace WpfAnalyzers;

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

        internal ArgumentSyntax TypeArgument => this.Invocation.ArgumentList.Arguments[0];

        internal ArgumentSyntax EventArgument => this.Invocation.ArgumentList.Arguments[1];

        internal ArgumentSyntax DelegateArgument => this.Invocation.ArgumentList.Arguments[2];

        internal static RegisterClassHandler? Match(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (invocation is { ArgumentList.Arguments.Count: 3 or 4 } &&
                semanticModel.TryGetSymbol(invocation, KnownSymbols.EventManager.RegisterClassHandler, cancellationToken, out var method))
            {
                return new RegisterClassHandler(invocation, method);
            }

            return null;
        }
    }

    internal readonly struct RegisterRoutedEvent
    {
        internal readonly InvocationExpressionSyntax Invocation;
        internal readonly IMethodSymbol Target;

        internal RegisterRoutedEvent(InvocationExpressionSyntax invocation, IMethodSymbol target)
        {
            this.Invocation = invocation;
            this.Target = target;
        }

        internal ArgumentSyntax? NameArgument => this.Invocation.ArgumentList.Arguments[0];

        internal ArgumentSyntax? OwnerTypeArgument => this.Invocation.ArgumentList.Arguments[3];

        internal static RegisterRoutedEvent? Match(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (invocation is { ArgumentList.Arguments.Count: 4 } &&
                semanticModel.TryGetSymbol(invocation, KnownSymbols.EventManager.RegisterRoutedEvent, cancellationToken, out var method))
            {
                return new RegisterRoutedEvent(invocation, method);
            }

            return null;
        }
    }

    internal readonly struct AddHandler
    {
        internal readonly InvocationExpressionSyntax Invocation;
        internal readonly IMethodSymbol Target;

        internal AddHandler(InvocationExpressionSyntax invocation, IMethodSymbol target)
        {
            this.Invocation = invocation;
            this.Target = target;
        }

        internal ArgumentSyntax EventArgument => this.Invocation.ArgumentList.Arguments[0];

        internal ArgumentSyntax DelegateArgument => this.Invocation.ArgumentList.Arguments[1];

        internal static AddHandler? Match(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (invocation is { ArgumentList.Arguments.Count: 2 or 3 } &&
                invocation.TryGetMethodName(out var name) &&
                name == "AddHandler" &&
                semanticModel.TryGetSymbol(invocation, cancellationToken, out var method) &&
                method.Parameters.TryFirst(out var parameter) &&
                parameter.Type == KnownSymbols.RoutedEvent)
            {
                return new AddHandler(invocation, method);
            }

            return null;
        }
    }

    internal readonly struct RemoveHandler
    {
        internal readonly InvocationExpressionSyntax Invocation;
        internal readonly IMethodSymbol Target;

        internal RemoveHandler(InvocationExpressionSyntax invocation, IMethodSymbol target)
        {
            this.Invocation = invocation;
            this.Target = target;
        }

        internal ArgumentSyntax EventArgument => this.Invocation.ArgumentList.Arguments[0];

        internal ArgumentSyntax DelegateArgument => this.Invocation.ArgumentList.Arguments[1];

        internal static RemoveHandler? Match(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (invocation is { ArgumentList.Arguments.Count: 2 } &&
                invocation.TryGetMethodName(out var name) &&
                name == "RemoveHandler" &&
                semanticModel.TryGetSymbol(invocation, cancellationToken, out var method) &&
                method.Parameters.TryFirst(out var parameter) &&
                parameter.Type == KnownSymbols.RoutedEvent)
            {
                return new RemoveHandler(invocation, method);
            }

            return null;
        }
    }
}
