namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class RoutedEvent
    {
        internal static bool TryGetRegisterCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return TryGetCall(
                invocation,
                KnownSymbol.EventManager.RegisterRoutedEvent,
                semanticModel,
                cancellationToken,
                out method);
        }

        internal static bool TryGetRegisteredName(FieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken, out string registeredName)
        {
            registeredName = null;
            if (fieldOrProperty.TryGetAssignedValue(cancellationToken, out var value) &&
                value is InvocationExpressionSyntax invocation)
            {
                if (TryGetRegisterCall(invocation, semanticModel, cancellationToken, out _) &&
                    invocation.TryGetArgumentAtIndex(0, out var nameArg))
                {
                    return nameArg.TryGetStringValue(semanticModel, cancellationToken, out registeredName);
                }
            }

            return false;
        }

        /// <summary>
        /// This is an optimization to avoid calling <see cref="SemanticModel.GetSymbolInfo"/>
        /// </summary>
        private static bool TryGetCall(InvocationExpressionSyntax invocation, QualifiedMethod qualifiedMethod, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            method = null;
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