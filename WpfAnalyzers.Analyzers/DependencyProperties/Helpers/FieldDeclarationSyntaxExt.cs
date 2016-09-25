namespace WpfAnalyzers.DependencyProperties
{
    using System;
    using System.Linq;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class FieldDeclarationSyntaxExt
    {
        internal static string DependencyPropertyRegisteredName(this FieldDeclarationSyntax declaration)
        {
            MemberAccessExpressionSyntax invocation;
            if (!TryGetRegisterInvocation(declaration, out invocation))
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
            }

            return null;
        }

        internal static string DependencyPropertyRegisteredType(this FieldDeclarationSyntax declaration)
        {
            MemberAccessExpressionSyntax invocation;
            if (!TryGetRegisterInvocation(declaration, out invocation))
            {
                return null;
            }

            throw new NotImplementedException();
        }

        internal static string DependencyPropertyRegisteredDefaultValue(this FieldDeclarationSyntax declaration)
        {
            throw new NotImplementedException();
        }

        private static bool TryGetRegisterInvocation(FieldDeclarationSyntax declaration, out MemberAccessExpressionSyntax invocation)
        {
            invocation = (declaration.Declaration
                    .Variables
                    .FirstOrDefault()
                    .Initializer.Value as InvocationExpressionSyntax)
                ?.Expression as MemberAccessExpressionSyntax;
            return invocation?.IsDependencyPropertyRegister() == true;
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