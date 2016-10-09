namespace WpfAnalyzers.DependencyProperties
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class DependencyPropertyField
    {
        internal static bool IsDependencyPropertyField(this FieldDeclarationSyntax declaration)
        {
            MemberAccessExpressionSyntax temp;
            return declaration.IsDependencyPropertyType() &&
                   (declaration.TryGetRegisterInvocation(out temp) ||
                    declaration.TryGetAddOwnerInvocation(out temp));
        }

        internal static bool IsDependencyPropertyKeyField(this FieldDeclarationSyntax declaration)
        {
            MemberAccessExpressionSyntax temp;
            return declaration.IsDependencyPropertyKeyType() &&
                   declaration.TryGetRegisterInvocation(out temp);
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

        internal static bool TryGetDependencyPropertyKey(this FieldDeclarationSyntax field, out FieldDeclarationSyntax result)
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

        internal static bool TryGetDependencyPropertyRegisteredName(this FieldDeclarationSyntax field, SemanticModel semanticModel, CancellationToken cancellationToken, out string result)
        {
            result = null;
            if (field == null)
            {
                return false;
            }

            MemberAccessExpressionSyntax registerCall;
            if (!TryGetRegisterInvocation(field, out registerCall))
            {
                return false;
            }

            return registerCall.TryGetRegisteredName(semanticModel, cancellationToken, out result);
        }

        internal static bool TryGetDependencyPropertyRegisteredType(this FieldDeclarationSyntax field, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol result)
        {
            result = null;
            MemberAccessExpressionSyntax registerCall;
            if (!TryGetRegisterInvocation(field, out registerCall))
            {
                return false;
            }

            return registerCall.TryGetRegisteredType(semanticModel, cancellationToken, out result);
        }

        internal static bool TryGetDependencyPropertyRegisteredOwnerType(this FieldDeclarationSyntax field, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax argument, out ITypeSymbol result)
        {
            argument = null;
            result = null;
            MemberAccessExpressionSyntax registerCall;
            if (TryGetRegisterInvocation(field, out registerCall))
            {
                return registerCall.TryGetRegisteredOwnerType(semanticModel, cancellationToken, out argument, out result);
            }

            if (TryGetAddOwnerInvocation(field, out registerCall))
            {
                var args = (registerCall.Parent as InvocationExpressionSyntax)?.ArgumentList;
                if (args == null || args.Arguments.Count < 1)
                {
                    return false;
                }

                argument = args.Arguments[0];
                return argument.TryGetType(semanticModel, cancellationToken, out result);
            }

            return false;
        }

        internal static bool TryGetIdentifier(this FieldDeclarationSyntax field, out SyntaxToken result)
        {
            var variables = field?.Declaration?.Variables;
            if (variables?.Count != 1)
            {
                result = default(SyntaxToken);
                return false;
            }

            var variable = variables.Value[0];
            result = variable.Identifier;
            return true;
        }

        internal static IFieldSymbol FieldSymbol(this FieldDeclarationSyntax field, SemanticModel semanticModel)
        {
            return (IFieldSymbol)semanticModel.GetDeclaredSymbol(field.Declaration.Variables[0]);
        }

        internal static bool TryGetRegisterInvocation(this FieldDeclarationSyntax declaration, out MemberAccessExpressionSyntax invocation)
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

        private static bool TryGetAddOwnerInvocation(this FieldDeclarationSyntax declaration, out MemberAccessExpressionSyntax invocation)
        {
            if (!TryGetInitializerCall(declaration, out invocation))
            {
                return false;
            }

            return invocation.IsDependencyPropertyAddOwner();
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
    }
}