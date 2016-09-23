namespace WpfAnalyzers.DependencyProperties
{
    using System;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class FieldDeclarationSyntaxExt
    {
        internal static string RegisteredDependencyPropertyName(this FieldDeclarationSyntax declaration)
        {
            var invocation = declaration.DescendantNodes()
                                        .OfType<VariableDeclaratorSyntax>()
                                        .FirstOrDefault()
                                        ?.DescendantNodes()
                                        .OfType<EqualsValueClauseSyntax>()
                                        .FirstOrDefault()
                                        ?.DescendantNodes()
                                        .OfType<MemberAccessExpressionSyntax>()
                                        .FirstOrDefault();
            if (invocation == null || !invocation.IsDependencyPropertyRegister())
            {
                return null;
            }

            var args = (invocation.Parent as InvocationExpressionSyntax)?.ArgumentList;
            var nameArg = args?.Arguments.FirstOrDefault();
            if (nameArg == null)
            {
                return null;
            }

            string result;
            if (TryGetStringLiteral(nameArg.Expression, out result))
            {
                return result;
            }

            var nameOfInvocation = nameArg.Expression as InvocationExpressionSyntax;
            if (nameOfInvocation?.IsNameOfInvocation() == true)
            {
                var argument = nameOfInvocation.ArgumentList.Arguments[0];
                var identifierName = argument.Expression as IdentifierNameSyntax;
                if (identifierName != null)
                {
                    return identifierName.Identifier.Text;
                }

                //var memberName = argument.Expression as MemberAccessExpressionSyntax;
                //if (memberName != null)
                //{
                //    return memberName.Name?.Identifier.Text;
                //}
            }

            return null;
        }

        private static bool TryGetStringLiteral(ExpressionSyntax expression, out string result)
        {
            var literal = expression as LiteralExpressionSyntax;
            if (literal == null || literal.Kind() != SyntaxKind.StringLiteralExpression)
            {
                result = null;
                return false;
            }

            result = literal.Token.ValueText;
            return true;
        }
    }

    internal static class InvocationExpressionSyntaxExt
    {
        internal static bool IsNameOfInvocation(this InvocationExpressionSyntax invocation)
        {
            if (invocation == null)
            {
                return false;
            }

            var identifier = invocation.Expression as IdentifierNameSyntax;
            return identifier?.Identifier.ValueText == "nameof";
        }
    }
}