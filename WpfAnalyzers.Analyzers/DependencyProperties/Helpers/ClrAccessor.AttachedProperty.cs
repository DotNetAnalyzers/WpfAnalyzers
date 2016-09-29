namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static partial class ClrAccessor
    {
        internal static bool TryGetDependencyPropertyRegisteredNameFromAttachedGet(this MethodDeclarationSyntax method, out string result)
        {
            result = null;
            FieldDeclarationSyntax dependencyProperty;
            if (AttachedPropertyHelper.TryGetFromGetMethod(method, out dependencyProperty))
            {
                return dependencyProperty.TryGetDependencyPropertyRegisteredName(out result);
            }

            return false;
        }

        internal static bool TryGetDependencyPropertyRegisteredNameFromAttachedSet(this MethodDeclarationSyntax method, out string result)
        {
            result = null;
            FieldDeclarationSyntax dependencyProperty;
            if (AttachedPropertyHelper.TryGetFromSetMethod(method, out dependencyProperty))
            {
                return dependencyProperty.TryGetDependencyPropertyRegisteredName(out result);
            }

            return false;
        }

        internal static bool TryGetDependencyPropertyRegisteredTypeFromAttachedGet(this MethodDeclarationSyntax method, out TypeSyntax result)
        {
            result = null;
            FieldDeclarationSyntax dependencyProperty;
            if (AttachedPropertyHelper.TryGetFromGetMethod(method, out dependencyProperty))
            {
                return dependencyProperty.TryGetDependencyPropertyRegisteredType(out result);
            }

            return false;
        }

        internal static bool TryGetDependencyPropertyRegisteredTypeFromAttachedSet(this MethodDeclarationSyntax method, out TypeSyntax result)
        {
            result = null;
            FieldDeclarationSyntax dependencyProperty;
            if (AttachedPropertyHelper.TryGetFromSetMethod(method, out dependencyProperty))
            {
                return dependencyProperty.TryGetDependencyPropertyRegisteredType(out result);
            }

            return false;
        }

        /// <summary>
        /// Small helper class to hide TryGetSingleChildInvocation that is not useful anywhere else.
        /// </summary>
        private static class AttachedPropertyHelper
        {
            internal static bool TryGetFromGetMethod(MethodDeclarationSyntax method, out FieldDeclarationSyntax result)
            {
                result = null;
                if (method == null ||
                    method.ReturnType.IsVoid() ||
                    method.Modifiers.Any(SyntaxKind.StaticKeyword) != true ||
                    method.Modifiers.Any(SyntaxKind.PublicKeyword) != true ||
                    method.ParameterList?.Parameters.Count != 1)
                {
                    return false;
                }

                ExpressionSyntax invocation;
                if (TryGetSingleChildInvocation(method, out invocation))
                {
                    ArgumentSyntax argument;
                    InvocationExpressionSyntax getValue;
                    if (invocation.TryGetGetValueInvocation(out getValue, out argument))
                    {
                        result = method.DeclaringType()
                                       .Field(argument.Expression as IdentifierNameSyntax);
                        FieldDeclarationSyntax dependencyPropertyKey;
                        if (result.TryGetDependencyPropertyKey(out dependencyPropertyKey))
                        {
                            result = dependencyPropertyKey;
                        }
                    }
                }

                return result != null;
            }

            internal static bool TryGetFromSetMethod(MethodDeclarationSyntax method, out FieldDeclarationSyntax result)
            {
                result = null;
                if (method == null ||
                    !method.ReturnType.IsVoid() ||
                    method.Modifiers.Any(SyntaxKind.StaticKeyword) != true ||
                    method.Modifiers.Any(SyntaxKind.PublicKeyword) != true ||
                    method.ParameterList?.Parameters.Count != 2)
                {
                    return false;
                }

                ExpressionSyntax invocation;
                if (TryGetSingleChildInvocation(method, out invocation))
                {
                    InvocationExpressionSyntax setValueCall;
                    ArgumentSyntax dpArg;
                    ArgumentSyntax arg;
                    if (invocation.TryGetSetValueInvocation(out setValueCall, out dpArg, out arg))
                    {
                        result = method.DeclaringType()
                                       .Field(dpArg.Expression as IdentifierNameSyntax);
                    }
                }

                return result != null;
            }

            private static bool TryGetSingleChildInvocation(MethodDeclarationSyntax method, out ExpressionSyntax result)
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
}