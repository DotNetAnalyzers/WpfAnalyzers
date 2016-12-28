namespace WpfAnalyzers.PropertyChanged.Helpers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Equality
    {
        internal static bool IsOperatorEquals(ExpressionSyntax condition, IdentifierNameSyntax first, IdentifierNameSyntax other)
        {
            var equals = condition as BinaryExpressionSyntax;
            if (equals?.IsKind(SyntaxKind.EqualsExpression) == true)
            {
                return IsLeftAndRight(equals, first, other);
            }

            return false;
        }

        internal static bool IsOperatorNotEquals(ExpressionSyntax condition, IdentifierNameSyntax first, IdentifierNameSyntax other)
        {
            var equals = condition as BinaryExpressionSyntax;
            if (equals?.IsKind(SyntaxKind.NotEqualsExpression) == true)
            {
                return IsLeftAndRight(equals, first, other);
            }

            return false;
        }

        private static bool IsLeftAndRight(BinaryExpressionSyntax equals, IdentifierNameSyntax first, IdentifierNameSyntax other)
        {
            if (IsIdentifier(equals.Left, first) && IsIdentifier(equals.Right, other))
            {
                return true;
            }

            if (IsIdentifier(equals.Left, other) && IsIdentifier(equals.Right, first))
            {
                return true;
            }

            return false;
        }

        private static bool IsIdentifier(ExpressionSyntax expression, IdentifierNameSyntax name)
        {
            if (name == null)
            {
                return false;
            }

            var identifier = expression as SimpleNameSyntax;
            if (identifier == null)
            {
                var memberAccess = expression as MemberAccessExpressionSyntax;
                if (memberAccess?.Expression is ThisExpressionSyntax)
                {
                    identifier = memberAccess.Name;
                }
            }

            return identifier?.Identifier.ValueText == name.Identifier.ValueText;
        }
    }
}
