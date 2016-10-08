namespace WpfAnalyzers
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ArgumentSyntaxExt
    {
        internal static bool TryGetString(this ArgumentSyntax argument, SemanticModel semanticModel, CancellationToken cancellationToken, out string result)
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
                var cv = semanticModel.GetConstantValue(argument.Expression, cancellationToken);
                if (cv.HasValue && cv.Value is string)
                {
                    result = (string)cv.Value;
                    return true;
                }
            }

            var symbolInfo = semanticModel.GetSymbolInfo(argument.Expression, cancellationToken);
            if (symbolInfo.Symbol?.ContainingType?.Name == "String" &&
                symbolInfo.Symbol?.Name == "Empty")
            {
                result = string.Empty;
                return true;
            }

            return false;
        }

        internal static bool TryGetType(this ArgumentSyntax argument, SemanticModel semanticModel, out ITypeSymbol result)
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

            var typeOf = argument.Expression as TypeOfExpressionSyntax;
            if (typeOf != null)
            {
                var typeSyntax = typeOf.Type;
                var typeInfo = semanticModel.GetTypeInfo(typeSyntax);
                result = typeInfo.Type;
                return result != null;
            }

            return false;
        }

        internal static bool TryGetDependencyPropertyFieldDeclaration(this ArgumentSyntax argument, SemanticModel semanticModel, CancellationToken cancellationToken, out FieldDeclarationSyntax result)
        {
            result = null;
            var dp = semanticModel.GetSymbolInfo(argument.Expression, cancellationToken);
            if (dp.Symbol.DeclaringSyntaxReferences.Length != 1)
            {
                return false;
            }

            var declarator = dp.Symbol.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) as VariableDeclaratorSyntax;
            if (declarator == null)
            {
                return false;
            }

            result = declarator.Parent?.Parent as FieldDeclarationSyntax;
            return result != null;
        }

        private static bool IsNameOf(this ExpressionSyntax expression)
        {
            return (expression as InvocationExpressionSyntax)?.IsNameOf() == true;
        }
    }
}