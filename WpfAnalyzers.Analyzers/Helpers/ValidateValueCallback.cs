namespace WpfAnalyzers
{
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
            if (DependencyProperty.TryGetRegisterCall(registerCall, semanticModel, cancellationToken, out var method) ||
                DependencyProperty.TryGetRegisterReadOnlyCall(registerCall, semanticModel, cancellationToken, out method) ||
                DependencyProperty.TryGetRegisterAttachedCall(registerCall, semanticModel, cancellationToken, out method) ||
                DependencyProperty.TryGetRegisterAttachedReadOnlyCall(registerCall, semanticModel, cancellationToken, out method))
            {
                return Argument.TryGetArgument(method.Parameters, registerCall.ArgumentList, KnownSymbol.ValidateValueCallback, out callback);
            }

            callback = null;
            return false;
        }
    }
}