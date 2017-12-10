namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class TypeOf
    {
        internal static bool TryGetType(TypeOfExpressionSyntax expression, INamedTypeSymbol containingType, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol type)
        {
            type = null;
            if (expression == null)
            {
                return false;
            }

            type = semanticModel.GetTypeInfoSafe(expression.Type, cancellationToken).Type;
            if (type.Kind == SymbolKind.TypeParameter)
            {
                while (containingType != null)
                {
                    if (containingType.IsGenericType)
                    {
                        var index = containingType.TypeParameters.IndexOf((ITypeParameterSymbol)type);
                        if (index >= 0)
                        {
                            type = containingType.TypeArguments[index];
                            return true;
                        }
                    }

                    containingType = containingType.ContainingType;
                }
            }

            return type != null;
        }
    }
}
