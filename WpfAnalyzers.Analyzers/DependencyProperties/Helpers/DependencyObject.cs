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
            IFieldSymbol __;
            return TryGetGetValueArgument(invocation, semanticModel, cancellationToken, out _, out __);
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

            property = invocation.ArgumentList.Arguments[0];
            field = semanticModel.SemanticModelFor(property.Expression)
                                 .GetSymbolInfo(property.Expression, cancellationToken)
                                 .Symbol as IFieldSymbol;
            return true;
        }

        internal static bool IsSetValue(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            ArgumentListSyntax _;
            return TryGetSetValueArguments(invocation, semanticModel, cancellationToken, out _);
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to dependencyObject.SetValue(FooProperty, value)
        /// </summary>
        internal static bool TryGetSetValueArguments(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentListSyntax arguments)
        {
            arguments = null;
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
            ArgumentListSyntax argumentList;
            if (TryGetSetValueArguments(invocation, semanticModel, cancellationToken, out argumentList))
            {
                property = argumentList.Arguments[0];
                value = argumentList.Arguments[1];
                field = semanticModel.SemanticModelFor(property.Expression)
                   .GetSymbolInfo(property.Expression, cancellationToken).Symbol as IFieldSymbol;
                return true;
            }

            property = null;
            value = null;
            field = null;
            return false;
        }

        internal static bool IsSetSetCurrentValue(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            ArgumentListSyntax _;
            return TryGetSetValueArguments(invocation, semanticModel, cancellationToken, out _);
        }

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to dependencyObject.SetCurrentValue(FooProperty, value)
        /// </summary>
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

        /// <summary>
        /// Check if <paramref name="invocation"/> is a call to dependencyObject.SetCurrentValue(FooProperty, value)
        /// </summary>
        /// <param name="property">The DependencyProperty used as argument</param>
        /// <param name="field">The field symbol for <paramref name="property"/></param>
        /// <param name="value">The value argument</param>
        internal static bool TryGetSetCurrentValueArguments(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax property, out IFieldSymbol field, out ArgumentSyntax value)
        {
            ArgumentListSyntax argumentList;
            if (TryGetSetCurrentValueArguments(invocation, semanticModel, cancellationToken, out argumentList))
            {
                property = argumentList.Arguments[0];
                value = argumentList.Arguments[1];
                field = semanticModel.SemanticModelFor(property.Expression)
                                     .GetSymbolInfo(property.Expression, cancellationToken)
                                     .Symbol as IFieldSymbol;
                return true;
            }

            property = null;
            value = null;
            field = null;
            return false;
        }
    }
}