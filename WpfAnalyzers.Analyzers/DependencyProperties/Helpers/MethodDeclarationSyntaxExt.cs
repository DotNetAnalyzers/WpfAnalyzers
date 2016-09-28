namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class MethodDeclarationSyntaxExt
    {
        internal static string Name(this MethodDeclarationSyntax method)
        {
            return method?.Identifier.ValueText;
        }

        internal static bool TryGetDependencyPropertyRegisteredNameFromAttachedGet(this MethodDeclarationSyntax method, out string result)
        {
            result = null;
            ExpressionSyntax invocation;
            if (TryGetSingleChildInvocation(method, out invocation))
            {
                ArgumentSyntax argument;
                InvocationExpressionSyntax getValue;
                if (invocation.TryGetGetValueInvocation(out getValue, out argument))
                {
                    var dependencyProperty = method.Class()
                                                   .Field(argument.Expression as IdentifierNameSyntax);
                    result = dependencyProperty?.DependencyPropertyRegisteredName();
                }
            }

            return result != null;
        }

        internal static bool TryGetDependencyPropertyRegisteredNameFromAttachedSet(this MethodDeclarationSyntax method, out string result)
        {
            result = null;
            ExpressionSyntax invocation;
            if (TryGetSingleChildInvocation(method, out invocation))
            {
                InvocationExpressionSyntax setValueCall;
                ArgumentSyntax dpArg;
                ArgumentSyntax arg;
                if (invocation.TryGetSetValueInvocation(out setValueCall, out dpArg, out arg))
                {
                    var dependencyProperty = method.Class()
                                                   .Field(dpArg.Expression as IdentifierNameSyntax);
                    result = dependencyProperty.DependencyPropertyRegisteredName();
                }
            }

            return result != null;
        }

        private static bool TryGetSingleChildInvocation(this MethodDeclarationSyntax method, out ExpressionSyntax result)
        {
            result = null;
            var count = method?.Body?.Statements.Count;
            if (count.HasValue && count != 1)
            {
                return false;
            }

            if (count == 1)
            {
                var statement = method.Body.Statements[0];
                var expressionStatement = statement as ExpressionStatementSyntax;
                if (expressionStatement != null)
                {
                    result = expressionStatement.Expression;
                    return result != null;
                }

                var returnStatement = statement as ReturnStatementSyntax;
                if (returnStatement != null)
                {
                    result = returnStatement.Expression;
                    return result != null;
                }
            }
            else
            {
                result = method?.ExpressionBody?.Expression;
            }

            return result != null;
        }
    }
}