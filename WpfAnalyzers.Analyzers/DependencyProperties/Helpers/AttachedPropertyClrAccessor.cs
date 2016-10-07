namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class AttachedPropertyClrAccessor
    {
        internal static bool TryGetGetValueInvocation(
            this ExpressionSyntax returnExpression,
            out InvocationExpressionSyntax getValue,
            out ArgumentSyntax dependencyProperty)
        {
            getValue = null;
            dependencyProperty = null;
            var cast = returnExpression as CastExpressionSyntax;
            if (cast != null)
            {
                return TryGetGetValueInvocation(cast.Expression, out getValue, out dependencyProperty);
            }

            var invocation = returnExpression as InvocationExpressionSyntax;
            if (invocation.Name() == Names.GetValue && invocation?.ArgumentList?.Arguments.Count == 1)
            {
                getValue = invocation;
                dependencyProperty = invocation.ArgumentList.Arguments[0];
            }

            return getValue != null;
        }

        internal static bool TryGetSetValueInvocation(
            this ExpressionSyntax returnExpression,
            out InvocationExpressionSyntax setValue,
            out ArgumentSyntax dependencyProperty,
            out ArgumentSyntax argument)
        {
            setValue = null;
            dependencyProperty = null;
            argument = null;
            var invocation = returnExpression as InvocationExpressionSyntax;
            if (invocation.Name() == Names.SetValue &&
                invocation?.ArgumentList?.Arguments.Count == 2)
            {
                setValue = invocation;
                dependencyProperty = invocation.ArgumentList.Arguments[0];
                argument = invocation.ArgumentList.Arguments[1];
            }

            return setValue != null;
        }

        internal static bool TryGetDependencyPropertyRegisteredNameFromAttachedGet(this MethodDeclarationSyntax method, SemanticModel semanticModel, out string result)
        {
            result = null;
            FieldDeclarationSyntax dependencyProperty;
            if (AttachedPropertyHelper.TryGetFromGetMethod(method, out dependencyProperty))
            {
                return dependencyProperty.TryGetDependencyPropertyRegisteredName(semanticModel, out result);
            }

            return false;
        }

        internal static bool TryGetDependencyPropertyRegisteredNameFromAttachedSet(this MethodDeclarationSyntax method, SemanticModel semanticModel, out string result)
        {
            result = null;
            FieldDeclarationSyntax dependencyProperty;
            if (AttachedPropertyHelper.TryGetFromSetMethod(method, out dependencyProperty))
            {
                return dependencyProperty.TryGetDependencyPropertyRegisteredName(semanticModel, out result);
            }

            return false;
        }

        internal static bool TryGetDependencyPropertyRegisteredTypeFromAttachedGet(this MethodDeclarationSyntax method, SemanticModel semanticModel, out ITypeSymbol result)
        {
            result = null;
            FieldDeclarationSyntax dependencyProperty;
            if (AttachedPropertyHelper.TryGetFromGetMethod(method, out dependencyProperty))
            {
                return dependencyProperty.TryGetDependencyPropertyRegisteredType(semanticModel, out result);
            }

            return false;
        }

        internal static bool TryGetDependencyPropertyRegisteredTypeFromAttachedSet(this MethodDeclarationSyntax method, SemanticModel semanticModel, out ITypeSymbol result)
        {
            result = null;
            FieldDeclarationSyntax dependencyProperty;
            if (AttachedPropertyHelper.TryGetFromSetMethod(method, out dependencyProperty))
            {
                return dependencyProperty.TryGetDependencyPropertyRegisteredType(semanticModel, out result);
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