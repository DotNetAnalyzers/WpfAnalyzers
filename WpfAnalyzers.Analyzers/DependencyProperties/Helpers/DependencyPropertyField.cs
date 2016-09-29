namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class DependencyPropertyField
    {
        internal static bool IsDependencyPropertyField(this FieldDeclarationSyntax declaration)
        {
            string temp;
            return declaration.IsDependencyPropertyType() &&
                   declaration.TryGetDependencyPropertyRegisteredName(out temp);
        }

        internal static bool IsDependencyPropertyKeyField(this FieldDeclarationSyntax declaration)
        {
            string temp;
            return declaration.IsDependencyPropertyKeyType() &&
                   declaration.TryGetDependencyPropertyRegisteredName(out temp);
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

            var name = (memberAccess?.Expression as IdentifierNameSyntax)?.Identifier.ValueText;
            result = field.DeclaringType().Field(name);
            return result != null;
        }

        internal static bool TryGetDependencyPropertyRegisteredName(this FieldDeclarationSyntax declaration, out string result)
        {
            result = null;
            if (declaration == null)
            {
                return false;
            }

            MemberAccessExpressionSyntax invocation;
            if (!TryGetRegisterInvocation(declaration, out invocation))
            {
                return false;
            }

            var args = (invocation.Parent as InvocationExpressionSyntax)?.ArgumentList;
            var nameArg = args?.Arguments.FirstOrDefault();
            if (nameArg == null)
            {
                return false;
            }

            if (TryGetStringLiteral(nameArg.Expression, out result))
            {
                return true;
            }

            if ((nameArg.Expression as InvocationExpressionSyntax)?.TryGetNameOfResult(out result) == true)
            {
                return true;
            }

            return false;
        }

        internal static bool TryGetDependencyPropertyRegisteredType(this FieldDeclarationSyntax declaration, out TypeSyntax result)
        {
            result = null;
            MemberAccessExpressionSyntax invocation;
            if (!TryGetRegisterInvocation(declaration, out invocation))
            {
                return false;
            }

            var args = (invocation.Parent as InvocationExpressionSyntax)?.ArgumentList;
            if (args == null || args.Arguments.Count < 2)
            {
                return false;
            }

            var typeArg = args.Arguments[1];
            if (typeArg == null)
            {
                return false;
            }

            return typeArg.Expression.TryGetTypeOfResult(out result);
        }

        internal static bool TryGetDependencyPropertyRegisteredOwnerType(this FieldDeclarationSyntax declaration, out TypeSyntax result)
        {
            result = null;
            MemberAccessExpressionSyntax invocation;
            if (!TryGetRegisterInvocation(declaration, out invocation))
            {
                return false;
            }

            var args = (invocation.Parent as InvocationExpressionSyntax)?.ArgumentList;
            if (args == null || args.Arguments.Count < 3)
            {
                return false;
            }

            var typeArg = args.Arguments[2];
            if (typeArg == null)
            {
                return false;
            }

            return typeArg.Expression.TryGetTypeOfResult(out result);
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