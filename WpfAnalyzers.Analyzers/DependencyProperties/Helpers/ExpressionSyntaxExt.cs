namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ExpressionSyntaxExt
    {
        internal static bool TryGetGetValueInvocation(
            this ExpressionSyntax returnExpression,
            out InvocationExpressionSyntax getValueInvocation,
            out ArgumentSyntax dependencyProperty)
        {
            dependencyProperty = null;
            getValueInvocation = null;
            var castExpressionSyntax = returnExpression as CastExpressionSyntax;
            if (castExpressionSyntax != null)
            {
                return TryGetGetValueInvocation(
                    castExpressionSyntax.Expression,
                    out getValueInvocation,
                    out dependencyProperty);
            }

            var invocation = returnExpression as InvocationExpressionSyntax;
            if (invocation.Name() == "GetValue" && invocation?.ArgumentList?.Arguments.Count == 1)
            {
                getValueInvocation = invocation;
                dependencyProperty = invocation.ArgumentList.Arguments[0];
            }

            return getValueInvocation != null;
        }

        internal static bool TryGetSetValueInvocation(
            this ExpressionSyntax returnExpression,
            out InvocationExpressionSyntax setValueInvocation,
            out ArgumentSyntax dependencyProperty,
            out ArgumentSyntax argument)
        {
            setValueInvocation = null;
            dependencyProperty = null;
            argument = null;
            var invocation = returnExpression as InvocationExpressionSyntax;
            if (invocation.Name() == "SetValue" && invocation?.ArgumentList?.Arguments.Count == 2)
            {
                setValueInvocation = invocation;
                dependencyProperty = invocation.ArgumentList.Arguments[0];
                argument = invocation.ArgumentList.Arguments[1];
            }

            return setValueInvocation != null;
        }
    }
}
