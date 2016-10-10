namespace WpfAnalyzers
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    using WpfAnalyzers.DependencyProperties;

    internal static class ArgumentSyntaxExt
    {
        internal static bool IsObject(this ArgumentSyntax argument, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (argument?.Expression == null)
            {
                return false;
            }

            var symbol = semanticModel.SemanticModelFor(argument.Expression)
                                      .GetTypeInfo(argument.Expression, cancellationToken)
                                      .Type;
            return symbol.IsObject();
        }

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

        internal static bool TryGetTypeofType(this ArgumentSyntax argument, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol result)
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

        internal static bool TryGetDependencyPropertyFieldDeclaration(this ArgumentSyntax argument, SemanticModel semanticModel, CancellationToken cancellationToken, out FieldDeclarationSyntax result)
        {
            result = null;
            var dp = semanticModel.SemanticModelFor(argument.Expression)
                                  .GetSymbolInfo(argument.Expression, cancellationToken);
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

        internal static bool TryGetDependencyPropertyRegistration(this ArgumentSyntax argument, SemanticModel semanticModel, CancellationToken cancellationToken, out MemberAccessExpressionSyntax result)
        {
            result = null;
            FieldDeclarationSyntax field;
            if (TryGetDependencyPropertyFieldDeclaration(argument, semanticModel, cancellationToken, out field))
            {
                return field.TryGetRegisterInvocation(out result);
            }

            return false;
        }

        private static bool IsNameOf(this ExpressionSyntax expression)
        {
            return (expression as InvocationExpressionSyntax)?.IsNameOf() == true;
        }
    }
}