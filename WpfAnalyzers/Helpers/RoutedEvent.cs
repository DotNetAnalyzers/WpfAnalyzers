namespace WpfAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class RoutedEvent
    {
        internal static bool TryGetRegisterCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out IMethodSymbol? method)
        {
            return semanticModel.TryGetSymbol(invocation, KnownSymbols.EventManager.RegisterRoutedEvent, cancellationToken, out method);
        }

        internal static bool TryGetRegisteredName(FieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ArgumentSyntax? nameArg, [NotNullWhen(true)] out string? result)
        {
            nameArg = null;
            result = null;
            if (fieldOrProperty.TryGetAssignedValue(cancellationToken, out var value) &&
                value is InvocationExpressionSyntax invocation)
            {
                if (TryGetRegisterCall(invocation, semanticModel, cancellationToken, out _) &&
                    invocation.TryGetArgumentAtIndex(0, out nameArg))
                {
                    return nameArg.TryGetStringValue(semanticModel, cancellationToken, out result);
                }
            }

            return false;
        }

        internal static bool TryGetRegisteredType(FieldOrProperty fieldOrProperty, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ArgumentSyntax? typeArg, [NotNullWhen(true)] out ITypeSymbol? result)
        {
            typeArg = null;
            result = null;
            if (fieldOrProperty.TryGetAssignedValue(cancellationToken, out var value) &&
                value is InvocationExpressionSyntax invocation)
            {
                if (TryGetRegisterCall(invocation, semanticModel, cancellationToken, out _) &&
                    invocation.TryGetArgumentAtIndex(3, out typeArg))
                {
                    return typeArg.TryGetTypeofValue(semanticModel, cancellationToken, out result);
                }
            }

            return false;
        }
    }
}
