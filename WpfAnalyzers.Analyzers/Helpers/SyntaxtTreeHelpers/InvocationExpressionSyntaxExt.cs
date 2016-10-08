namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class InvocationExpressionSyntaxExt
    {
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

        internal static bool IsGetValue(this InvocationExpressionSyntax invocation)
        {
            return invocation.Name() == Names.GetValue && invocation?.ArgumentList?.Arguments.Count == 1;
        }

        internal static bool IsSetValue(this InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            ArgumentSyntax _;
            ArgumentSyntax __;
            return TryGetSetValueArguments(invocation, semanticModel, out _, out __);
        }

        internal static bool TryGetSetValueArguments(this InvocationExpressionSyntax invocation, SemanticModel semanticModel, out ArgumentSyntax property, out ArgumentSyntax value)
        {
            property = null;
            value = null;
            if (invocation.Name() != Names.SetValue || invocation?.ArgumentList?.Arguments.Count != 2)
            {
                return false;
            }

            var symbol = semanticModel.GetSymbolInfo(invocation).Symbol;
            if (symbol?.ContainingSymbol?.Name != Names.DependencyObject)
            {
                return false;
            }

            property = invocation.ArgumentList.Arguments[0];
            value = invocation.ArgumentList.Arguments[1];
            return true;
        }

        internal static bool IsSetSetCurrentValue(this InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            ArgumentSyntax _;
            ArgumentSyntax __;
            return TryGetSetValueArguments(invocation, semanticModel, out _, out __);
        }

        internal static bool TryGetSetCurrentValueArguments(this InvocationExpressionSyntax invocation, SemanticModel semanticModel, out ArgumentSyntax property, out ArgumentSyntax value)
        {
            property = null;
            value = null;
            if (invocation.Name() != Names.SetCurrentValue || invocation?.ArgumentList?.Arguments.Count != 2)
            {
                return false;
            }

            var symbol = semanticModel.GetSymbolInfo(invocation).Symbol;
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