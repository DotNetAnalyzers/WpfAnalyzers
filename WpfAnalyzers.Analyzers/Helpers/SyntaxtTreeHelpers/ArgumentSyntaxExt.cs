namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ArgumentSyntaxExt
    {
        internal static bool TryGetString(this ArgumentSyntax argument, SemanticModel semanticModel, out string result)
        {
            result = null;
            if (argument?.Expression == null)
            {
                return false;
            }

            if (argument.Expression.IsKind(SyntaxKind.NullLiteralExpression))
            {
                return true;
            }

            if (argument.Expression.IsKind(SyntaxKind.StringLiteralExpression) ||
                argument.Expression.IsNameOf())
            {
                var cv = semanticModel.GetConstantValue(argument.Expression);
                if (cv.HasValue && cv.Value is string)
                {
                    result = (string)cv.Value;
                    return true;
                }
            }

            var symbolInfo = semanticModel.GetSymbolInfo(argument.Expression);
            if (symbolInfo.Symbol?.ContainingType?.Name == "String" &&
                symbolInfo.Symbol?.Name == "Empty")
            {
                result = string.Empty;
                return true;
            }

            return false;
        }

        internal static bool TryGetString(this ArgumentSyntax argument, out string result)
        {
            result = null;
            if (argument == null)
            {
                return false;
            }

            if (TryGetStringLiteral(argument.Expression, out result))
            {
                return true;
            }

            if ((argument.Expression as InvocationExpressionSyntax)?.TryGetNameOfResult(out result) == true)
            {
                return true;
            }

            return false;
        }

        private static bool TryGetStringLiteral(ExpressionSyntax expression, out string result)
        {
            var literal = expression as LiteralExpressionSyntax;
            if (literal == null || literal.Kind() != SyntaxKind.StringLiteralExpression)
            {
                result = null;
                return false;
            }

            result = literal.Token.ValueText;
            return true;
        }

        private static bool IsNameOf(this ExpressionSyntax expression)
        {
            return (expression as InvocationExpressionSyntax)?.IsNameOf() == true;
        }
    }
}
