namespace WpfAnalyzers.DependencyProperties
{
    using System;
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class ValidateValueCallback
    {
        internal static bool TryGetName(ArgumentSyntax callback, SemanticModel semanticModel, CancellationToken cancellationToken, out IdentifierNameSyntax nameExpression, out string name)
        {
            return Callback.TryGetName(
                callback,
                KnownSymbol.ValidateValueCallback,
                semanticModel,
                cancellationToken,
                out nameExpression,
                out name);
        }

        internal static bool TryGetRegisteredName(ArgumentSyntax callback, SemanticModel semanticModel, CancellationToken cancellationToken, out string registeredName)
        {
            return Callback.TryGetRegisteredName(callback, semanticModel, cancellationToken, out registeredName);
        }

        internal static bool TryGetValidateValueCallback(InvocationExpressionSyntax registerCall, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax callback)
        {
            callback = null;
            var methodSymbol = semanticModel.SemanticModelFor(registerCall)
                                            .GetSymbolInfo(registerCall, cancellationToken)
                                            .Symbol as IMethodSymbol;
            if (methodSymbol == null ||
                methodSymbol.ContainingType != KnownSymbol.DependencyProperty)
            {
                return false;
            }

            if (!methodSymbol.Name.StartsWith("Register", StringComparison.Ordinal))
            {
                return false;
            }

            for (int i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                var parameter = methodSymbol.Parameters[i];
                if (parameter.Type == KnownSymbol.ValidateValueCallback)
                {
                    return registerCall.TryGetArgumentAtIndex(i, out callback);
                }
            }

            return false;
        }
    }
}