// ReSharper disable UnusedMember.Global
namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal static class ExpressionSyntaxExt
    {
        internal static bool IsSameType(this ExpressionSyntax expression, QualifiedType metadataName, SyntaxNodeAnalysisContext context)
        {
            return expression.IsSameType(metadataName, context.SemanticModel, context.CancellationToken);
        }

        internal static bool IsSameType(this ExpressionSyntax expression, QualifiedType metadataName, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var type = semanticModel?.Compilation.GetTypeByMetadataName(metadataName.FullName);
            return expression.IsSameType(type, semanticModel, cancellationToken);
        }

        internal static bool IsSameType(this ExpressionSyntax expression, ITypeSymbol type, SemanticModel semanticModel, CancellationToken cancellationToken)
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
