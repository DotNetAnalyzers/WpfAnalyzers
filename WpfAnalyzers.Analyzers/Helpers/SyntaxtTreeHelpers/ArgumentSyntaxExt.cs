namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ArgumentSyntaxExt
    {
        internal static bool TryGetStringValue(this ArgumentSyntax argument, SemanticModel semanticModel, CancellationToken cancellationToken, out string result)
        {
            result = null;
            if (argument?.Expression == null || semanticModel == null)
            {
                return false;
            }

            switch (argument.Expression)
            {
            case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression):
                result = literal.Token.ValueText;
                return true;
            case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.NullLiteralExpression):
                result = null;
                return true;
            case InvocationExpressionSyntax invocation when invocation.IsNameOf():
                if (invocation.ArgumentList != null &&
                    invocation.ArgumentList.Arguments.TrySingle(out var nameofArg))
                {
                    switch (nameofArg.Expression)
                    {
                    case IdentifierNameSyntax identifierName:
                        result = identifierName.Identifier.ValueText;
                        return true;
                    case MemberAccessExpressionSyntax memberAccess:
                        result = memberAccess.Name.Identifier.ValueText;
                        return true;
                    default:
                        var constantValue = semanticModel.GetConstantValueSafe(invocation, cancellationToken);
                        if (constantValue.HasValue && constantValue.Value is string)
                        {
                            result = (string)constantValue.Value;
                            return true;
                        }

                        break;
                    }
                }

                return false;
            case MemberAccessExpressionSyntax memberAccess when semanticModel.GetSymbolSafe(memberAccess, cancellationToken) == KnownSymbol.String.Empty:
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

            if (argument.Expression is TypeOfExpressionSyntax typeOf)
            {
                var typeSyntax = typeOf.Type;
                var typeInfo = semanticModel.SemanticModelFor(typeSyntax)
                                            .GetTypeInfo(typeSyntax, cancellationToken);
                result = typeInfo.Type;
                return result != null;
            }

            return false;
        }
    }
}
