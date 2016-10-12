namespace WpfAnalyzers
{
    using System;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class InvocationExpressionSyntaxExt
    {
        internal static bool TryGetArgumentAtIndex(
            this InvocationExpressionSyntax invocation,
            int index,
            out ArgumentSyntax result)
        {
            result = null;
            if (invocation?.ArgumentList?.Arguments == null)
            {
                return false;
            }

            if (invocation.ArgumentList.Arguments.Count <= index)
            {
                return false;
            }

            result = invocation.ArgumentList.Arguments[index];
            return true;
        }

        [Obsolete("Use symbols")]
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
                        var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                        if (memberAccess != null)
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