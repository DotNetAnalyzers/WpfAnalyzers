namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class TypeOf
    {
        internal static bool TryGetType(TypeOfExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol type)
        {
            type = null;
            if (expression == null)
            {
                return false;
            }

            type = semanticModel.GetTypeInfoSafe(expression.Type, cancellationToken).Type;
            return type != null;
        }
    }
}
