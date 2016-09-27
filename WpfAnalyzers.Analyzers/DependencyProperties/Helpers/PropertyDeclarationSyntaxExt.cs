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

        internal static string DependencyPropertyRegisteredName(this PropertyDeclarationSyntax property)
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

            var arg = invocation.ArgumentList?.Arguments.FirstOrDefault()?.Expression as IdentifierNameSyntax;
            if (arg == null)
            {
                dependencyProperty = null;
                return false;
            }

            var classDeclaration = (ClassDeclarationSyntax)propertyDeclaration.Parent;
            foreach (var member in classDeclaration.Members)
            {
                var field = member as FieldDeclarationSyntax;
                if (!field.IsDependencyPropertyType())
                {
                    continue;
                }

                if (field.Name() == arg.Identifier.Text)
                {
                    dependencyProperty = field;
                    return true;
                }
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
    }
}