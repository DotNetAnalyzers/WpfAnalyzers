namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class ValidateValueCallback
    {
        internal static bool TryGetName(ArgumentSyntax callback, SemanticModel semanticModel, CancellationToken cancellationToken, out IdentifierNameSyntax identifier, out string name)
        {
            return Callback.TryGetName(
                callback,
                KnownSymbol.ValidateValueCallback,
                semanticModel,
                cancellationToken,
                out identifier,
                out name);
        }

        internal static bool TryGetRegisteredName(ArgumentSyntax callback, SemanticModel semanticModel, CancellationToken cancellationToken, out string registeredName)
        {
            return DependencyProperty.TryGetRegisteredName(callback?.FirstAncestorOrSelf<InvocationExpressionSyntax>(), semanticModel, cancellationToken, out registeredName);
        }

        internal static bool TryGetValidateValueCallback(InvocationExpressionSyntax registerCall, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax callback)
        {
            callback = null;
            if (DependencyProperty.TryGetRegisterCall(registerCall, semanticModel, cancellationToken, out var method) ||
                DependencyProperty.TryGetRegisterReadOnlyCall(registerCall, semanticModel, cancellationToken, out method) ||
                DependencyProperty.TryGetRegisterAttachedCall(registerCall, semanticModel, cancellationToken, out method) ||
                DependencyProperty.TryGetRegisterAttachedReadOnlyCall(registerCall, semanticModel, cancellationToken, out method))
            {
                return method.TryFindParameter(KnownSymbol.ValidateValueCallback, out var parameter) &&
                       registerCall.TryFindArgument(parameter, out callback);
            }

            return false;
        }
    }
}
