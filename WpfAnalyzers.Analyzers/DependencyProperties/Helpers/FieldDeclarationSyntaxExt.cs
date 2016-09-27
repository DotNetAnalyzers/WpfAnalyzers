namespace WpfAnalyzers.DependencyProperties
{
    using System;

    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class FieldDeclarationSyntaxExt
    {
        internal static bool IsDependencyPropertyType(this FieldDeclarationSyntax declaration)
        {
            var type = declaration?.Declaration?.Type as IdentifierNameSyntax;
            return type?.Identifier.Text == Names.DependencyProperty;
        }

        internal static bool IsDependencyPropertyKeyType(this FieldDeclarationSyntax declaration)
        {
            var type = declaration?.Declaration?.Type as IdentifierNameSyntax;
            return type?.Identifier.Text == Names.DependencyPropertyKey;
        }

        internal static string Name(this FieldDeclarationSyntax declaration)
        {
            var variables = declaration?.Declaration?.Variables;
            if (variables?.Count != 1)
            {
                return null;
            }

            return variables.Value[0].Identifier.Text;
        }

        internal static FieldDeclarationSyntax DependencyPropertyKey(this FieldDeclarationSyntax field)
        {
            if (!field.IsDependencyPropertyType())
            {
                return null;
            }

            var declarationSyntax = field.Declaration;
            if (declarationSyntax == null || declarationSyntax.Variables.Count != 1)
            {
                return null;
            }

            var variable = declarationSyntax?.Variables.FirstOrDefault();
            if (variable == null)
            {
                return null;
            }

            var memberAccess = variable.Initializer.Value as MemberAccessExpressionSyntax;
            if (!memberAccess.IsDependencyPropertyKeyProperty())
            {
                return null;
            }

            var classSyntax = (ClassDeclarationSyntax)field.Parent;
            var name = (memberAccess?.Expression as IdentifierNameSyntax)?.Identifier.Text;
            return classSyntax.FieldDeclaration(name);
        }

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

            if ((nameArg.Expression as InvocationExpressionSyntax)?.TryGetNameOfResult(out result) == true)
            {
                return result;
            }

            return null;
        }

        internal static TypeSyntax DependencyPropertyRegisteredType(this FieldDeclarationSyntax declaration)
        {
            MemberAccessExpressionSyntax invocation;
            if (!TryGetRegisterInvocation(declaration, out invocation))
            {
                return null;
            }

            var args = (invocation.Parent as InvocationExpressionSyntax)?.ArgumentList;
            if (args == null || args.Arguments.Count < 2)
            {
                return null;
            }

            var typeArg = args.Arguments[1];
            if (typeArg == null)
            {
                return null;
            }

            TypeSyntax result;
            if (typeArg.Expression.TryGetTypeOfResult(out result))
            {
                return result;
            }

            return null;
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
}