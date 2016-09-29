namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ExpressionSyntaxExt
    {
        internal static bool TryGetGetValueInvocation(
            this ExpressionSyntax returnExpression,
            out InvocationExpressionSyntax getValue,
            out ArgumentSyntax dependencyProperty)
        {
            getValue = null;
            dependencyProperty = null;
            var cast = returnExpression as CastExpressionSyntax;
            if (cast != null)
            {
                return TryGetGetValueInvocation(
                    cast.Expression,
                    out getValue,
                    out dependencyProperty);
            }

            var invocation = returnExpression as InvocationExpressionSyntax;
            if (invocation.Name() == Names.GetValue && invocation?.ArgumentList?.Arguments.Count == 1)
            {
                getValue = invocation;
                dependencyProperty = invocation.ArgumentList.Arguments[0];
            }

            return getValue != null;
        }

        internal static bool TryGetSetValueInvocation(
            this ExpressionSyntax returnExpression,
            out InvocationExpressionSyntax setValue,
            out ArgumentSyntax dependencyProperty,
            out ArgumentSyntax argument)
        {
            setValue = null;
            dependencyProperty = null;
            argument = null;
            var invocation = returnExpression as InvocationExpressionSyntax;
            if (invocation.Name() == Names.SetValue &&
                invocation?.ArgumentList?.Arguments.Count == 2)
            {
                setValue = invocation;
                dependencyProperty = invocation.ArgumentList.Arguments[0];
                argument = invocation.ArgumentList.Arguments[1];
            }

            return setValue != null;
        }
    }
}
