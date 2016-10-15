namespace WpfAnalyzers
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ArgumentSyntaxExt
    {
        internal static bool TryGetSymbol<T>(this ArgumentSyntax argument, SemanticModel semanticModel, CancellationToken cancellationToken, out T result)
            where T : class, ISymbol
        {
            result = semanticModel.SemanticModelFor(argument.Expression)
                                  .GetSymbolInfo(argument.Expression, cancellationToken)
                                  .Symbol as T;

            return result != null;
        }

        internal static bool TryGetStringValue(this ArgumentSyntax argument, SemanticModel semanticModel, CancellationToken cancellationToken, out string result)
        {
            result = null;
            if (argument?.Expression == null || semanticModel == null)
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
                var cv = semanticModel.SemanticModelFor(argument.Expression)
                                      .GetConstantValue(argument.Expression, cancellationToken);
                if (cv.HasValue && cv.Value is string)
                {
                    result = (string)cv.Value;
                    return true;
                }
            }

            var symbolInfo = semanticModel.SemanticModelFor(argument.Expression)
                                          .GetSymbolInfo(argument.Expression, cancellationToken);
            if (symbolInfo.Symbol?.ContainingType?.Name == "String" &&
                symbolInfo.Symbol?.Name == "Empty")
            {
                result = string.Empty;
                return true;
            }

            return false;
        }

        internal static bool TryGetTypeofValue(this ArgumentSyntax argument, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol result)
        {
            result = null;
            if (argument?.Expression == null || semanticModel == null)
            {
                return false;
            }

            var typeOf = argument.Expression as TypeOfExpressionSyntax;
            if (typeOf != null)
            {
                var typeSyntax = typeOf.Type;
                var typeInfo = semanticModel.SemanticModelFor(typeSyntax)
                                            .GetTypeInfo(typeSyntax, cancellationToken);
                result = typeInfo.Type;
                return result != null;
            }

            return false;
        }

        private static bool IsNameOf(this ExpressionSyntax expression)
        {
            return (expression as InvocationExpressionSyntax)?.IsNameOf() == true;
        }
    }
}