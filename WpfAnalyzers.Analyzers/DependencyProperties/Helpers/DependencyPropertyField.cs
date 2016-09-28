namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class DependencyPropertyField
    {
        internal static bool IsDependencyPropertyField(this FieldDeclarationSyntax declaration)
        {
            return declaration.IsDependencyPropertyType() && declaration.DependencyPropertyRegisteredName() != null;
        }

        internal static bool IsDependencyPropertyKeyField(this FieldDeclarationSyntax declaration)
        {
            return declaration.IsDependencyPropertyKeyType() && declaration.DependencyPropertyRegisteredName() != null;
        }

        internal static bool IsDependencyPropertyType(this FieldDeclarationSyntax declaration)
        {
            var type = declaration?.Declaration?.Type as IdentifierNameSyntax;
            return type?.Identifier.ValueText == Names.DependencyProperty;
        }

        internal static bool IsDependencyPropertyKeyType(this FieldDeclarationSyntax declaration)
        {
            var type = declaration?.Declaration?.Type as IdentifierNameSyntax;
            return type?.Identifier.ValueText == Names.DependencyPropertyKey;
        }

        internal static bool TryGetDependencyPropertyKey(
            this FieldDeclarationSyntax field,
            out FieldDeclarationSyntax result)
        {
            result = null;
            if (!field.IsDependencyPropertyType())
            {
                return false;
            }

            var declarationSyntax = field.Declaration;
            if (declarationSyntax == null || declarationSyntax.Variables.Count != 1)
            {
                return false;
            }

            var variable = declarationSyntax.Variables.FirstOrDefault();
            if (variable == null)
            {
                return false;
            }

            var memberAccess = variable.Initializer.Value as MemberAccessExpressionSyntax;
            if (!memberAccess.IsDependencyPropertyKeyProperty())
            {
                return false;
            }

            var classSyntax = (ClassDeclarationSyntax)field.Parent;
            var name = (memberAccess?.Expression as IdentifierNameSyntax)?.Identifier.ValueText;
            result = classSyntax.Field(name);
            return result != null;
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

        internal static TypeSyntax DependencyPropertyRegisteredOwnerType(this FieldDeclarationSyntax declaration)
        {
            MemberAccessExpressionSyntax invocation;
            if (!TryGetRegisterInvocation(declaration, out invocation))
            {
                return null;
            }

            var args = (invocation.Parent as InvocationExpressionSyntax)?.ArgumentList;
            if (args == null || args.Arguments.Count < 3)
            {
                return null;
            }

            var typeArg = args.Arguments[2];
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

        private static bool TryGetRegisterInvocation(
            FieldDeclarationSyntax declaration,
            out MemberAccessExpressionSyntax invocation)
        {
            if (!TryGetInitializerCall(declaration, out invocation))
            {
                return false;
            }

            if (invocation.IsDependencyPropertyRegister() || invocation.IsDependencyPropertyRegisterReadOnly() ||
                invocation.IsDependencyPropertyRegisterAttached() ||
                invocation.IsDependencyPropertyRegisterAttachedReadOnly())
            {
                return true;
            }

            FieldDeclarationSyntax propertyKey;
            if (!declaration.TryGetDependencyPropertyKey(out propertyKey))
            {
                return false;
            }

            if (!TryGetInitializerCall(propertyKey, out invocation))
            {
                return false;
            }

            if (invocation.IsDependencyPropertyRegisterReadOnly() ||
                invocation.IsDependencyPropertyRegisterAttachedReadOnly())
            {
                return true;
            }

            return false;
        }

        private static bool TryGetInitializerCall(FieldDeclarationSyntax field, out MemberAccessExpressionSyntax result)
        {
            var initializer = field?.Declaration?.Variables.FirstOrDefault()
                                   ?.Initializer?.Value;
            result = initializer as MemberAccessExpressionSyntax;
            if (result != null)
            {
                return true;
            }

            var invocation = initializer as InvocationExpressionSyntax;
            if (invocation != null)
            {
                result = invocation.Expression as MemberAccessExpressionSyntax;
            }

            return result != null;
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