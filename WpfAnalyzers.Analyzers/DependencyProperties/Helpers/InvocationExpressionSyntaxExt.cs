﻿namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class InvocationExpressionSyntaxExt
    {
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
                        if (memberAccess != null && memberAccess.Expression is ThisExpressionSyntax)
                        {
                            identifierName = memberAccess.Name as IdentifierNameSyntax;
                        }
                    }

                    return identifierName?.Identifier.Text;
                default:
                    return null;
            }
        }

        internal static bool IsNameOfInvocation(this InvocationExpressionSyntax invocation)
        {
            return invocation.Name() == "nameof";
        }

        internal static bool TryGetNameOfResult(this InvocationExpressionSyntax nameOfInvocation, out string result)
        {
            if (nameOfInvocation == null)
            {
                result = null;
                return false;
            }

            if (nameOfInvocation.IsNameOfInvocation())
            {
                var argument = nameOfInvocation.ArgumentList.Arguments[0];
                var identifierName = argument.Expression as IdentifierNameSyntax;
                if (identifierName != null)
                {
                    result = identifierName.Identifier.Text;
                    return true;
                }
            }

            result = null;
            return false;
        }

        internal static bool TryGetTypeOfResult(this ExpressionSyntax invocation, out TypeSyntax result)
        {
            var typeOfExpression = invocation as TypeOfExpressionSyntax;
            if (typeOfExpression == null)
            {
                result = null;
                return false;
            }

            result = typeOfExpression.Type;
            return true;
        }
    }
}