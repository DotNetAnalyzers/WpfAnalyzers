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

        internal static AccessorDeclarationSyntax GetAccessorDeclaration(this PropertyDeclarationSyntax property)
        {
            var accessors = property?.AccessorList?.Accessors;
            if (accessors == null)
            {
                return null;
            }

            foreach (var accessor in accessors)
            {
                if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
                {
                    return accessor;
                }
            }

            return null;
        }

        internal static AccessorDeclarationSyntax SetAccessorDeclaration(this PropertyDeclarationSyntax property)
        {
            var accessors = property?.AccessorList?.Accessors;
            if (accessors == null)
            {
                return null;
            }

            foreach (var accessor in accessors)
            {
                if (accessor.IsKind(SyntaxKind.SetAccessorDeclaration))
                {
                    return accessor;
                }
            }

            return null;
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
            this PropertyDeclarationSyntax propertyDeclaration,
            out FieldDeclarationSyntax dependencyProperty)
        {
            var getter = propertyDeclaration.GetAccessorDeclaration();
            var returnStatement = getter.Body?.Statements.FirstOrDefault() as ReturnStatementSyntax;
            var invocation = GetValueInvocation(returnStatement?.Expression);
            if (invocation == null)
            {
                dependencyProperty = null;
                return false;
            }

            dependencyProperty = GetFieldFromFirstArgument(propertyDeclaration, invocation.ArgumentList);
            return dependencyProperty != null;
        }

        internal static bool TryGetDependencyPropertyFromSetter(this PropertyDeclarationSyntax propertyDeclaration, out FieldDeclarationSyntax dependencyProperty)
        {
            var setter = propertyDeclaration.SetAccessorDeclaration();
            var statement = setter?.Body?.Statements.FirstOrDefault() as ExpressionStatementSyntax;
            var invocation = statement?.Expression as InvocationExpressionSyntax;
            if (invocation == null)
            {
                dependencyProperty = null;
                return false;
            }

            if (invocation.Name() == "SetValue" && invocation.ArgumentList.Arguments.Count == 2)
            {
                dependencyProperty = GetFieldFromFirstArgument(propertyDeclaration, invocation.ArgumentList);
                return dependencyProperty != null;
            }

            dependencyProperty = null;
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

        private static FieldDeclarationSyntax GetFieldFromFirstArgument(PropertyDeclarationSyntax property, ArgumentListSyntax arguments)
        {
            if (arguments == null || arguments.Arguments.Count == 0)
            {
                return null;
            }

            var arg = arguments.Arguments[0].Expression as IdentifierNameSyntax;
            var classDeclaration = (ClassDeclarationSyntax)property.Parent;
            return classDeclaration.FieldDeclaration(arg?.Identifier.Text);
        }
    }
}