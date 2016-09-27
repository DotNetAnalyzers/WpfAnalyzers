namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyDeclarationSyntaxExt
    {
        internal static string Name(this PropertyDeclarationSyntax property)
        {
            return property?.Identifier.Text;
        }

        internal static string DependencyPropertyRegisteredNameFromGetter(this PropertyDeclarationSyntax property)
        {
            FieldDeclarationSyntax dependencyProperty;
            if (property.TryGetDependencyPropertyFromGetter(out dependencyProperty))
            {
                return dependencyProperty.DependencyPropertyRegisteredName();
            }

            return null;
        }

        internal static TypeSyntax DependencyPropertyRegisteredType(this PropertyDeclarationSyntax property)
        {
            FieldDeclarationSyntax dependencyProperty;
            if (property.TryGetDependencyPropertyFromGetter(out dependencyProperty))
            {
                return dependencyProperty.DependencyPropertyRegisteredType();
            }

            return null;
        }

        internal static bool TryGetGetAccessorDeclaration(this PropertyDeclarationSyntax property, out AccessorDeclarationSyntax result)
        {
            result = null;
            var accessors = property?.AccessorList?.Accessors;
            if (accessors == null)
            {
                return false;
            }

            foreach (var accessor in accessors)
            {
                if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
                {
                    result = accessor;
                    return true;
                }
            }

            return false;
        }

        internal static bool TryGetSetAccessorDeclaration(this PropertyDeclarationSyntax property, out AccessorDeclarationSyntax result)
        {
            result = null;
            var accessors = property?.AccessorList?.Accessors;
            if (accessors == null)
            {
                return false;
            }

            foreach (var accessor in accessors)
            {
                if (accessor.IsKind(SyntaxKind.SetAccessorDeclaration))
                {
                    result = accessor;
                    return true;
                }
            }

            return false;
        }

        internal static FieldDeclarationSyntax GetDependencyPropertyFromGetter(this PropertyDeclarationSyntax propertyDeclaration)
        {
            FieldDeclarationSyntax dp;
            return TryGetDependencyPropertyFromGetter(propertyDeclaration, out dp)
                       ? dp
                       : null;
        }

        internal static FieldDeclarationSyntax GetDependencyPropertyFromSetter(this PropertyDeclarationSyntax propertyDeclaration)
        {
            FieldDeclarationSyntax dp;
            return TryGetDependencyPropertyFromSetter(propertyDeclaration, out dp)
                       ? dp
                       : null;
        }

        internal static bool TryGetDependencyPropertyFromGetter(
            this PropertyDeclarationSyntax property,
            out FieldDeclarationSyntax dependencyProperty)
        {
            dependencyProperty = null;
            AccessorDeclarationSyntax getter;
            if (!property.TryGetGetAccessorDeclaration(out getter))
            {
                return false;
            }

            var returnStatement = getter.Body?.Statements.FirstOrDefault() as ReturnStatementSyntax;
            var invocation = GetValueInvocation(returnStatement?.Expression);
            if (invocation == null || invocation.ArgumentList.Arguments.Count != 1)
            {
                return false;
            }

            dependencyProperty = property.Class()
                .Field(invocation.ArgumentList.Arguments.First().Expression as IdentifierNameSyntax);
            return dependencyProperty != null;
        }

        internal static bool TryGetDependencyPropertyFromSetter(this PropertyDeclarationSyntax property, out FieldDeclarationSyntax dependencyProperty)
        {
            dependencyProperty = null;
            AccessorDeclarationSyntax setter;
            if (!property.TryGetSetAccessorDeclaration(out setter))
            {
                return false;
            }

            var statement = setter?.Body?.Statements.FirstOrDefault() as ExpressionStatementSyntax;
            var invocation = statement?.Expression as InvocationExpressionSyntax;
            if (invocation == null)
            {
                return false;
            }

            if (invocation.Name() == "SetValue" && invocation.ArgumentList.Arguments.Count == 2)
            {
                dependencyProperty = property.Class()
                                             .Field(invocation.ArgumentList.Arguments.First().Expression as IdentifierNameSyntax);
                return dependencyProperty != null;
            }

            return false;
        }

        private static InvocationExpressionSyntax GetValueInvocation(ExpressionSyntax returnExpression)
        {
            var castExpressionSyntax = returnExpression as CastExpressionSyntax;
            if (castExpressionSyntax != null)
            {
                return GetValueInvocation(castExpressionSyntax.Expression);
            }

            var invocation = returnExpression as InvocationExpressionSyntax;
            if (invocation.Name() == "GetValue" && invocation?.ArgumentList.Arguments.Count == 1)
            {
                return invocation;
            }

            return null;
        }
    }
}