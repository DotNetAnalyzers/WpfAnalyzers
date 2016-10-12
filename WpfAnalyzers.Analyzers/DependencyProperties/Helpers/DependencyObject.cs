namespace WpfAnalyzers.DependencyProperties
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class DependencyObject
    {
        internal static bool IsGetValue(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            ArgumentSyntax _;
            return TryGetGetValueArgument(invocation, semanticModel, cancellationToken, out _);
        }

        internal static bool TryGetGetValueArgument(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax result)
        {
            result = null;
            if (invocation == null)
            {
                return false;
            }

            var setter = semanticModel.SemanticModelFor(invocation)
                                            .GetSymbolInfo(invocation, cancellationToken).Symbol as IMethodSymbol;
            if (setter?.ContainingSymbol.Name != Names.DependencyObject ||
                setter.Name != Names.GetValue ||
                setter.Parameters.Length != 1)
            {
                return false;
            }

            result = invocation.ArgumentList.Arguments[0];
            return true;
        }

        internal static bool IsSetValue(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            ArgumentSyntax _;
            ArgumentSyntax __;
            return TryGetSetValueArguments(invocation, semanticModel, cancellationToken, out _, out __);
        }

        internal static bool TryGetSetValueArguments(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentListSyntax result)
        {
            result = null;
            if (invocation == null)
            {
                return false;
            }

            var setter = semanticModel.SemanticModelFor(invocation)
                                            .GetSymbolInfo(invocation, cancellationToken).Symbol as IMethodSymbol;
            if (setter?.ContainingSymbol.Name != Names.DependencyObject ||
                setter.Name != Names.SetValue ||
                setter.Parameters.Length != 2)
            {
                return false;
            }

            result = invocation.ArgumentList;
            return true;
        }

        internal static bool TryGetSetValueArguments(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax property, out ArgumentSyntax value)
        {
            ArgumentListSyntax argumentList;
            if (TryGetSetValueArguments(invocation, semanticModel, cancellationToken, out argumentList))
            {
                property = argumentList.Arguments[0];
                value = argumentList.Arguments[1];
                return true;
            }

            property = null;
            value = null;
            return false;
        }

        internal static bool IsSetSetCurrentValue(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            ArgumentSyntax _;
            ArgumentSyntax __;
            return TryGetSetValueArguments(invocation, semanticModel, cancellationToken, out _, out __);
        }

        internal static bool TryGetSetCurrentValueArguments(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentListSyntax result)
        {
            result = null;
            if (invocation == null)
            {
                return false;
            }

            var setter = semanticModel.SemanticModelFor(invocation)
                                            .GetSymbolInfo(invocation, cancellationToken).Symbol as IMethodSymbol;
            if (setter?.ContainingSymbol.Name != Names.DependencyObject ||
                setter.Name != Names.SetCurrentValue ||
                setter.Parameters.Length != 2)
            {
                return false;
            }

            result = invocation.ArgumentList;
            return true;
        }

        internal static bool TryGetSetCurrentValueArguments(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax property, out ArgumentSyntax value)
        {
            ArgumentListSyntax argumentList;
            if (TryGetSetCurrentValueArguments(invocation, semanticModel, cancellationToken, out argumentList))
            {
                property = argumentList.Arguments[0];
                value = argumentList.Arguments[1];
                return true;
            }

            property = null;
            value = null;
            return false;
        }
    }
}