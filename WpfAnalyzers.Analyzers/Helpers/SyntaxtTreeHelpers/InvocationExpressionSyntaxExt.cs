namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class InvocationExpressionSyntaxExt
    {
        internal static bool TrySingleArgument(this InvocationExpressionSyntax invocation, out ArgumentSyntax result)
        {
            result = null;
            return invocation?.ArgumentList?.Arguments.TrySingle(out result) == true;
        }

        internal static bool TryGetArgumentAtIndex(this InvocationExpressionSyntax invocation, int index, out ArgumentSyntax result)
        {
            result = null;
            return invocation?.ArgumentList?.Arguments.TryElementAt(index, out result) == true;
        }

        internal static bool TryGetInvokedMethodName(this InvocationExpressionSyntax invocation, out string name)
        {
            name = null;
            if (invocation == null)
            {
                return false;
            }

            switch (invocation.Kind())
            {
                case SyntaxKind.InvocationExpression:
                case SyntaxKind.SimpleMemberAccessExpression:
                case SyntaxKind.TypeOfExpression:
                    if (invocation.Expression is SimpleNameSyntax simple)
                    {
                        name = simple.Identifier.ValueText;
                        return true;
                    }

                    if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Name is SimpleNameSyntax member)
                    {
                        name = member.Identifier.ValueText;
                        return true;
                    }

                    if (invocation.Expression is MemberBindingExpressionSyntax memberBinding &&
                        memberBinding.Name is SimpleNameSyntax bound)
                    {
                        name = bound.Identifier.ValueText;
                        return true;
                    }

                    return false;
                default:
                    return false;
            }
        }

        internal static bool IsNameOf(this InvocationExpressionSyntax invocation)
        {
            return invocation.TryGetInvokedMethodName(out var name) &&
                    name == "nameof";
        }
    }
}