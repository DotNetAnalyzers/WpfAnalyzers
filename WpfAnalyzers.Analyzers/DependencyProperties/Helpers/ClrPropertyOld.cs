namespace WpfAnalyzers.DependencyProperties
{
    using System;

    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ClrPropertyOld
    {
        [Obsolete]
        internal static bool TryGetDependencyPropertyFromSetter(this AccessorDeclarationSyntax setter, out FieldDeclarationSyntax dependencyProperty)
        {
            dependencyProperty = null;
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
            if (invocation.TryGetSetValueArguments(out setValueCall, out dpArg, out arg))
            {
                dependencyProperty = setter.DeclaringType()
                                           .Field(dpArg.Expression as IdentifierNameSyntax);
                return dependencyProperty != null;
            }

            return false;
        }
    }
}