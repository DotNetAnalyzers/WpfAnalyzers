namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class InvocationExpressionSyntaxExt
    {
        internal static bool TryGetArgumentAtIndex(this InvocationExpressionSyntax invocation, int index, out ArgumentSyntax result)
        {
            result = null;
            return invocation?.ArgumentList?.Arguments.TryGetAtIndex(index, out result) == true;
        }

        internal static string InvokedMethodName(this InvocationExpressionSyntax invocation)
        {
            if (invocation == null)
            {
                return null;
            }

            switch (invocation.Kind())
            {
                case SyntaxKind.InvocationExpression:
                case SyntaxKind.SimpleMemberAccessExpression:
                case SyntaxKind.TypeOfExpression:
                    if (invocation.Expression is IdentifierNameSyntax simple)
                    {
                        return simple.Identifier.ValueText;
                    }

                    if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Name is IdentifierNameSyntax member)
                    {
                        return member.Identifier.ValueText;
                    }

                    if (invocation.Expression is MemberBindingExpressionSyntax memberBinding &&
                        memberBinding.Name is IdentifierNameSyntax bound)
                    {
                        return bound.Identifier.ValueText;
                    }

                    return null;
                default:
                    return null;
            }
        }

        internal static bool IsNameOf(this InvocationExpressionSyntax invocation)
        {
            return invocation.InvokedMethodName() == "nameof";
        }
    }
}