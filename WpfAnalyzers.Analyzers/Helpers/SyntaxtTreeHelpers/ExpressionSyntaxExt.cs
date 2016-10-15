namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ExpressionSyntaxExt
    {
        internal static bool Is(this ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, string metadataName)
        {
            var type = semanticModel?.Compilation.GetTypeByMetadataName(metadataName);
            return expression.Is(semanticModel, cancellationToken, type);
        }

        internal static bool Is(this ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, ITypeSymbol type)
        {
            if (expression == null || type == null)
            {
                return false;
            }

            var symbol = semanticModel.SemanticModelFor(expression)
                                      .GetTypeInfo(expression, cancellationToken)
                                      .Type;
            return symbol.Is(type);
        }

        internal static bool IsSameType(this ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, string metadataName)
        {
            var type = semanticModel?.Compilation.GetTypeByMetadataName(metadataName);
            return expression.IsSameType(semanticModel, cancellationToken, type);
        }

        internal static bool IsSameType(this ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, ITypeSymbol type)
        {
            if (expression == null || type == null)
            {
                return false;
            }

            var symbol = semanticModel.SemanticModelFor(expression)
                                      .GetTypeInfo(expression, cancellationToken)
                                      .Type;
            return symbol.IsSameType(type);
        }
    }
}