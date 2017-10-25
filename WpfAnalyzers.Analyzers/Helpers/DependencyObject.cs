#pragma warning disable 1573
namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class DependencyObject
    {
        internal static bool TryGetSetValueCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return TryGetCall(
                invocation,
                KnownSymbol.DependencyObject.SetValue,
                2,
                semanticModel,
                cancellationToken,
                out method);
        }

        internal static bool TryGetSetCurrentValueCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return TryGetCall(
                invocation,
                KnownSymbol.DependencyObject.SetCurrentValue,
                2,
                semanticModel,
                cancellationToken,
                out method);
        }

        internal static bool TryGetGetValueCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            return TryGetCall(
                invocation,
                KnownSymbol.DependencyObject.GetValue,
                1,
                semanticModel,
                cancellationToken,
                out method);
        }

        /// <summary>
        /// This is an optimization to avoid calling <see cref="SemanticModel.GetSymbolInfo"/>
        /// </summary>
        internal static bool IsPotentialSetValueCall(InvocationExpressionSyntax invocation)
        {
            if (invocation.TryGetInvokedMethodName(out var name))
            {
                return name == "SetValue";
            }

            return true;
        }

        /// <summary>
        /// This is an optimization to avoid calling <see cref="SemanticModel.GetSymbolInfo"/>
        /// </summary>
        internal static bool IsPotentialSetCurrentValueCall(InvocationExpressionSyntax invocation)
        {
            if (invocation.TryGetInvokedMethodName(out var name))
            {
                return name == "SetCurrentValue";
            }

            return true;
        }

        /// <summary>
        /// This is an optimization to avoid calling <see cref="SemanticModel.GetSymbolInfo"/>
        /// </summary>
        internal static bool IsPotentialGetValueCall(InvocationExpressionSyntax invocation)
        {
            if (invocation.TryGetInvokedMethodName(out var name))
            {
                return name == "GetValue";
            }

            return true;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to dependencyObject.GetValue(FooProperty, value)
        /// </summary>
        /// <param name="property">The DependencyProperty used as argument</param>
        /// <param name="field">The field symbol for <paramref name="property"/></param>
        internal static bool TryGetGetValueArgument(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax property, out IFieldSymbol field)
        {
            property = null;
            field = null;
            if (!IsPotentialGetValueCall(invocation))
            {
                return false;
            }

            var method = semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol;
            if (method != KnownSymbol.DependencyObject.GetValue ||
                method?.Parameters.Length != 1)
            {
                return false;
            }

            property = invocation.ArgumentList.Arguments[0];
            field = semanticModel.GetSymbolSafe(property.Expression, cancellationToken) as IFieldSymbol;
            return true;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to dependencyObject.SetValue(FooProperty, value)
        /// </summary>
        internal static bool TryGetSetValueArguments(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentListSyntax arguments)
        {
            arguments = null;
            if (!IsPotentialSetValueCall(invocation))
            {
                return false;
            }

            var method = semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol;
            if (method != KnownSymbol.DependencyObject.SetValue ||
                method?.Parameters.Length != 2)
            {
                return false;
            }

            arguments = invocation.ArgumentList;
            return true;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to dependencyObject.SetValue(FooProperty, value)
        /// </summary>
        /// <param name="property">The DependencyProperty used as argument</param>
        /// <param name="field">The field symbol for <paramref name="property"/></param>
        /// <param name="value">The value argument</param>
        internal static bool TryGetSetValueArguments(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax property, out IFieldSymbol field, out ArgumentSyntax value)
        {
            if (TryGetSetValueArguments(invocation, semanticModel, cancellationToken, out var argumentList))
            {
                property = argumentList.Arguments[0];
                value = argumentList.Arguments[1];
                field = semanticModel.GetSymbolSafe(property.Expression, cancellationToken) as IFieldSymbol;
                return true;
            }

            property = null;
            value = null;
            field = null;
            return false;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to dependencyObject.SetCurrentValue(FooProperty, value)
        /// </summary>
        internal static bool TryGetSetCurrentValueArguments(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentListSyntax result)
        {
            result = null;
            if (!IsPotentialSetCurrentValueCall(invocation))
            {
                return false;
            }

            var method = semanticModel.GetSymbolSafe(invocation, cancellationToken) as IMethodSymbol;
            if (method != KnownSymbol.DependencyObject.SetCurrentValue ||
                method?.Parameters.Length != 2)
            {
                return false;
            }

            result = invocation.ArgumentList;
            return true;
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to dependencyObject.SetCurrentValue(FooProperty, value)
        /// </summary>
        /// <param name="property">The DependencyProperty used as argument</param>
        /// <param name="field">The field symbol for <paramref name="property"/></param>
        /// <param name="value">The value argument</param>
        internal static bool TryGetSetCurrentValueArguments(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax property, out IFieldSymbol field, out ArgumentSyntax value)
        {
            if (TryGetSetCurrentValueArguments(invocation, semanticModel, cancellationToken, out var argumentList))
            {
                property = argumentList.Arguments[0];
                value = argumentList.Arguments[1];
                field = semanticModel.GetSymbolSafe(property.Expression, cancellationToken) as IFieldSymbol;
                return true;
            }

            property = null;
            value = null;
            field = null;
            return false;
        }

        /// <summary>
        /// This is an optimization to avoid calling <see cref="SemanticModel.GetSymbolInfo"/>
        /// </summary>
        private static bool TryGetCall(InvocationExpressionSyntax invocation, QualifiedMethod qualifiedMethod, int expectedArgs, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol method)
        {
            method = null;
            if (invocation == null ||
                invocation.ArgumentList == null ||
                invocation.ArgumentList.Arguments.Count != expectedArgs)
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