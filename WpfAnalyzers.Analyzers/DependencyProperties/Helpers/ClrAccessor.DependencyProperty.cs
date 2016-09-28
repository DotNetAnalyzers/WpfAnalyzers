namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static partial class ClrAccessor
    {
        internal static bool TryGetDependencyProperty(this PropertyDeclarationSyntax property, out FieldDeclarationSyntax dependencyProperty)
        {
            dependencyProperty = null;
            FieldDeclarationSyntax getter;
            FieldDeclarationSyntax setter;
            if (TryGetDependencyPropertyFromGetter(property, out getter) &&
                TryGetDependencyPropertyFromSetter(property, out setter))
            {
                if (getter == setter)
                {
                    dependencyProperty = getter;
                    return true;
                }
            }

            return false;
        }

        internal static bool TryGetDependencyPropertyRegisteredName(this PropertyDeclarationSyntax property, out string result)
        {
            result = null;
            FieldDeclarationSyntax dependencyProperty;
            if (property.TryGetDependencyProperty(out dependencyProperty))
            {
                result = dependencyProperty.DependencyPropertyRegisteredName();
            }

            return result != null;
        }

        internal static bool TryGetDependencyPropertyRegisteredType(this PropertyDeclarationSyntax property, out TypeSyntax result)
        {
            result = null;
            FieldDeclarationSyntax dependencyProperty;
            if (property.TryGetDependencyProperty(out dependencyProperty))
            {
                result = dependencyProperty.DependencyPropertyRegisteredType();
            }

            return result != null;
        }

        internal static bool TryGetDependencyPropertyFromGetter(this PropertyDeclarationSyntax property, out FieldDeclarationSyntax dependencyProperty)
        {
            dependencyProperty = null;
            AccessorDeclarationSyntax getter;
            if (!property.TryGetGetAccessorDeclaration(out getter))
            {
                return false;
            }

            var returnStatement = getter.Body?.Statements.FirstOrDefault() as ReturnStatementSyntax;
            InvocationExpressionSyntax invocation;
            ArgumentSyntax argument = null;
            if (returnStatement?.Expression.TryGetGetValueInvocation(out invocation, out argument) != true)
            {
                return false;
            }

            dependencyProperty = property.Class()
                                         .Field(argument.Expression as IdentifierNameSyntax);
            FieldDeclarationSyntax dependencyPropertyKey;
            if (dependencyProperty.TryGetDependencyPropertyKey(out dependencyPropertyKey))
            {
                dependencyProperty = dependencyPropertyKey;
            }

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

            var statements = setter?.Body?.Statements;
            if (statements?.Count != 1)
            {
                return false;
            }

            var statement = statements.Value[0] as ExpressionStatementSyntax;
            var invocation = statement?.Expression as InvocationExpressionSyntax;
            if (invocation == null)
            {
                return false;
            }

            InvocationExpressionSyntax setValueCall;
            ArgumentSyntax dpArg;
            ArgumentSyntax arg;
            if (invocation.TryGetSetValueInvocation(out setValueCall, out dpArg, out arg))
            {
                dependencyProperty = property.Class()
                                             .Field(dpArg.Expression as IdentifierNameSyntax);
                return dependencyProperty != null;
            }

            return false;
        }
    }
}