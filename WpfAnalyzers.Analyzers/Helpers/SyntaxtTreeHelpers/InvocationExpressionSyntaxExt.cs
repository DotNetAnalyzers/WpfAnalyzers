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

        internal static string Name(this InvocationExpressionSyntax invocation)
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
                    var identifierName = invocation.Expression as IdentifierNameSyntax;
                    if (identifierName == null)
                    {
                        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                        {
                            identifierName = memberAccess.Name as IdentifierNameSyntax;
                        }
                    }

                    return identifierName?.Identifier.ValueText;
                default:
                    return null;
            }
        }

        internal static bool IsNameOf(this InvocationExpressionSyntax invocation)
        {
            return invocation.Name() == "nameof";
        }
    }
}