namespace WpfAnalyzers
{
    using System;
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class InvocationExpressionSyntaxExt
    {
        internal static bool TryGetArgumentAtIndex(
            this InvocationExpressionSyntax invocation,
            int index,
            out ArgumentSyntax result)
        {
            result = null;
            if (invocation?.ArgumentList?.Arguments == null)
            {
                return false;
            }

            if (invocation.ArgumentList.Arguments.Count <= index)
            {
                return false;
            }

            result = invocation.ArgumentList.Arguments[index];
            return true;
        }

        [Obsolete("Use symbols")]
        internal static string Name(this InvocationExpressionSyntax invocation)
        {
            if (invocation == null)
            {
                return null;
            }

            switch (invocation.Kind())
            {
                case SyntaxKind.InvocationExpression:
                case SyntaxKind.SimpleMemberAccessExpression:
                case SyntaxKind.TypeOfExpression:
                    var identifierName = invocation.Expression as IdentifierNameSyntax;
                    if (identifierName == null)
                    {
                        var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                        if (memberAccess != null)
                        {
                            identifierName = memberAccess.Name as IdentifierNameSyntax;
                        }
                    }

                    return identifierName?.Identifier.ValueText;
                default:
                    return null;
            }
        }

        internal static bool IsNameOf(this InvocationExpressionSyntax invocation)
        {
            return invocation.Name() == "nameof";
        }

        internal static bool IsGetValue(this InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            ArgumentSyntax _;
            return TryGetGetValueArgument(invocation, semanticModel, cancellationToken, out _);
        }

        internal static bool TryGetGetValueArgument(this InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax property)
        {
            property = null;
            if (invocation.Name() != Names.GetValue || invocation?.ArgumentList?.Arguments.Count != 1)
            {
                return false;
            }

            var symbol = semanticModel.SemanticModelFor(invocation)
                                      .GetSymbolInfo(invocation, cancellationToken)
                                      .Symbol;
            if (symbol?.ContainingSymbol?.Name != Names.DependencyObject)
            {
                return false;
            }

            property = invocation.ArgumentList.Arguments[0];
            return true;
        }

        internal static bool IsSetValue(this InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            ArgumentSyntax _;
            ArgumentSyntax __;
            return TryGetSetValueArguments(invocation, semanticModel, cancellationToken, out _, out __);
        }

        internal static bool TryGetSetValueArguments(this InvocationExpressionSyntax invocation,
                                                     SemanticModel semanticModel,
                                                     CancellationToken cancellationToken,
                                                     out ArgumentListSyntax result)
        {
            result = null;
            if (invocation == null)
            {
                return false;
            }

            var setter = semanticModel.SemanticModelFor(invocation)
                                            .GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (setter?.ContainingSymbol.Name != Names.DependencyObject ||
                setter.Name != Names.SetValue ||
                setter.Parameters.Length != 2)
            {
                return false;
            }

            result = invocation.ArgumentList;
            return true;
        }

        internal static bool TryGetSetValueArguments(this InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax property, out ArgumentSyntax value)
        {
            ArgumentListSyntax argumentList;
            if (TryGetSetValueArguments(invocation, semanticModel, cancellationToken, out argumentList))
            {
                property = argumentList.Arguments[0];
                value = argumentList.Arguments[1];
                return true;
            }

            property = null;
            value = null;
            return false;
        }

        internal static bool IsSetSetCurrentValue(this InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            ArgumentSyntax _;
            ArgumentSyntax __;
            return TryGetSetValueArguments(invocation, semanticModel, cancellationToken, out _, out __);
        }

        internal static bool TryGetSetCurrentValueArguments(this InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax property, out ArgumentSyntax value)
        {
            property = null;
            value = null;
            if (invocation.Name() != Names.SetCurrentValue || invocation?.ArgumentList?.Arguments.Count != 2)
            {
                return false;
            }

            var symbol = semanticModel.SemanticModelFor(invocation)
                                      .GetSymbolInfo(invocation, cancellationToken)
                                      .Symbol;
            if (symbol?.ContainingSymbol?.Name != Names.DependencyObject)
            {
                return false;
            }

            property = invocation.ArgumentList.Arguments[0];
            value = invocation.ArgumentList.Arguments[1];
            return true;
        }
    }
}