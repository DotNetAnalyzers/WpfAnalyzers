namespace WpfAnalyzers.DependencyProperties
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class DependencyPropertyField
    {
        internal static bool IsDependencyPropertyType(this FieldDeclarationSyntax declaration)
        {
            var type = declaration?.Declaration?.Type as IdentifierNameSyntax;
            return type?.Identifier.ValueText == Names.DependencyProperty;
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

            var variable = declarationSyntax.Variables.First();

            var memberAccess = variable.Initializer.Value as MemberAccessExpressionSyntax;
            if (!memberAccess.IsDependencyPropertyKeyProperty())
            {
                return false;
            }

            var name = (memberAccess?.Expression as IdentifierNameSyntax)?.Identifier.ValueText;
            if (name == null)
            {
                return false;
            }

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
            if (!TryGetRegisterCall(field, out registerCall))
            {
                return false;
            }

            return registerCall.TryGetRegisteredName(semanticModel, cancellationToken, out result);
        }

        internal static bool TryGetRegisterCall(this FieldDeclarationSyntax declaration, out MemberAccessExpressionSyntax memberAccess)
        {
            if (!TryGetInitializerCall(declaration, out memberAccess))
            {
                return false;
            }

            if (memberAccess.IsDependencyPropertyRegister() ||
                memberAccess.IsDependencyPropertyRegisterReadOnly() ||
                memberAccess.IsDependencyPropertyRegisterAttached() ||
                memberAccess.IsDependencyPropertyRegisterAttachedReadOnly())
            {
                return true;
            }

            FieldDeclarationSyntax propertyKey;
            if (!declaration.TryGetDependencyPropertyKey(out propertyKey))
            {
                return false;
            }

            if (!TryGetInitializerCall(propertyKey, out memberAccess))
            {
                return false;
            }

            if (memberAccess.IsDependencyPropertyRegisterReadOnly() ||
                memberAccess.IsDependencyPropertyRegisterAttachedReadOnly())
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
    }
}