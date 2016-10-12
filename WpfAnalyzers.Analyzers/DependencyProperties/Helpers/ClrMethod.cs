namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ClrMethod
    {
        internal static bool TryGetGetValueInvocation(ExpressionSyntax expression, out InvocationExpressionSyntax getValue, out ArgumentSyntax property)
        {
            getValue = null;
            property = null;
            
            var cast = expression as CastExpressionSyntax;
            if (cast != null)
            {
                return TryGetGetValueInvocation(cast.Expression, out getValue, out property);
            }

            var invocation = expression as InvocationExpressionSyntax;
            if (invocation.Name() == Names.GetValue && invocation?.ArgumentList?.Arguments.Count == 1)
            {
                getValue = invocation;
                property = invocation.ArgumentList.Arguments[0];
            }

            return getValue != null;
        }
    }
}