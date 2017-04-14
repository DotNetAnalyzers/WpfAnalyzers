namespace WpfAnalyzers.PropertyChanged.Helpers
{
    using System;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Equality
    {
        internal static bool IsOperatorEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            var equals = condition as BinaryExpressionSyntax;
            if (equals?.IsKind(SyntaxKind.EqualsExpression) == true)
            {
                return IsLeftAndRight(equals, semanticModel, cancellationToken, first, other);
            }

            return false;
        }

        internal static bool IsOperatorNotEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            var equals = condition as BinaryExpressionSyntax;
            if (equals?.IsKind(SyntaxKind.NotEqualsExpression) == true)
            {
                return IsLeftAndRight(equals, semanticModel, cancellationToken, first, other);
            }

            return false;
        }

        internal static bool IsObjectEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            var equals = condition as InvocationExpressionSyntax;
            var method = semanticModel.GetSymbolSafe(@equals, cancellationToken) as IMethodSymbol;
            if (method?.Parameters.Length == 2 &&
                method == KnownSymbol.Object.Equals)
            {
                return IsArguments(equals, semanticModel, cancellationToken, first, other);
            }

            return false;
        }

        internal static bool IsEqualityComparerEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            var equals = condition as InvocationExpressionSyntax;
            var memberAccess = equals?.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null)
            {
                return false;
            }

            var method = semanticModel.GetSymbolSafe(equals, cancellationToken) as IMethodSymbol;
            if (method?.Parameters.Length == 2 &&
                method.Name == "Equals")
            {
                var type = semanticModel.GetTypeInfoSafe(memberAccess.Expression, cancellationToken).Type;
                return type?.MetadataName == "EqualityComparer`1" &&
                       IsArguments(equals, semanticModel, cancellationToken, first, other);
            }

            return false;
        }

        internal static bool IsInstanceEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol instance, ISymbol arg)
        {
            var equals = condition as InvocationExpressionSyntax;
            var memberAccess = equals?.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null)
            {
                return false;
            }

            var method = semanticModel.GetSymbolSafe(equals, cancellationToken) as IMethodSymbol;
            if (method?.Parameters.Length == 1 &&
                method.Name == "Equals")
            {
                return instance.Equals(semanticModel.GetSymbolSafe(memberAccess.Expression, cancellationToken)) &&
                       IsArgument(equals, semanticModel, cancellationToken, arg);
            }

            return false;
        }

        internal static bool IsNullableEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            var equals = condition as InvocationExpressionSyntax;
            var method = semanticModel.GetSymbolSafe(@equals, cancellationToken) as IMethodSymbol;
            if (method?.Parameters.Length == 2 &&
                method == KnownSymbol.Nullable.Equals)
            {
                return IsArguments(equals, semanticModel, cancellationToken, first, other);
            }

            return false;
        }

        internal static bool IsReferenceEquals(ExpressionSyntax condition, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            var equals = condition as InvocationExpressionSyntax;
            var method = semanticModel.GetSymbolSafe(@equals, cancellationToken) as IMethodSymbol;
            if (method?.Parameters.Length == 2 &&
                method == KnownSymbol.Object.ReferenceEquals)
            {
                return IsArguments(equals, semanticModel, cancellationToken, first, other);
            }

            return false;
        }

        internal static bool UsesObjectOrNone(ExpressionSyntax condition)
        {
            if (condition is PrefixUnaryExpressionSyntax unary)
            {
                return UsesObjectOrNone(unary.Operand);
            }

            var memberAccess = (condition as InvocationExpressionSyntax)?.Expression as MemberAccessExpressionSyntax;
            if (memberAccess?.Expression is IdentifierNameSyntax identifierName &&
    !string.Equals(identifierName.Identifier.ValueText, "object", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private static bool IsArguments(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            if (invocation?.ArgumentList.Arguments.Count < 2)
            {
                return false;
            }

            return IsArgument(invocation, semanticModel, cancellationToken, first) &&
                   IsArgument(invocation, semanticModel, cancellationToken, other);
        }

        private static bool IsArgument(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol expected)
        {
            if (invocation == null || invocation.ArgumentList.Arguments.Count < 1)
            {
                return false;
            }

            foreach (var argument in invocation.ArgumentList.Arguments)
            {
                var symbol = semanticModel.GetSymbolSafe(argument.Expression, cancellationToken);
                if (expected.Equals(symbol))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsLeftAndRight(BinaryExpressionSyntax equals, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol first, ISymbol other)
        {
            if (IsIdentifier(equals.Left, semanticModel, cancellationToken, first) &&
                IsIdentifier(equals.Right, semanticModel, cancellationToken, other))
            {
                return true;
            }

            if (IsIdentifier(equals.Left, semanticModel, cancellationToken, other) &&
                IsIdentifier(equals.Right, semanticModel, cancellationToken, first))
            {
                return true;
            }

            return false;
        }

        private static bool IsIdentifier(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, ISymbol expected)
        {
            if (expected == null)
            {
                return false;
            }

            return expected.Equals(semanticModel.GetSymbolSafe(expression, cancellationToken));
        }
    }
}
