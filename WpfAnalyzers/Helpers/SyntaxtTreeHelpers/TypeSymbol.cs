namespace WpfAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class TypeSymbol
    {
        internal static bool TryGet(TypeOfExpressionSyntax expression, INamedTypeSymbol containingType, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ITypeSymbol? type)
        {
            if (expression is { Type: { } typeSyntax })
            {
                type = TryGet(typeSyntax, containingType, semanticModel, cancellationToken);
                return type is { };
            }

            type = null;
            return false;
        }

        internal static ITypeSymbol? TryGet(TypeSyntax typeSyntax, INamedTypeSymbol containingType, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (semanticModel.GetType(typeSyntax, cancellationToken) is { } type)
            {
                if (type.Kind == SymbolKind.TypeParameter)
                {
                    while (containingType is { })
                    {
                        if (containingType.IsGenericType &&
                            containingType != KnownSymbols.Object)
                        {
                            var index = containingType.TypeParameters.IndexOf((ITypeParameterSymbol)type);
                            if (index >= 0)
                            {
                                return containingType.TypeArguments[index];
                            }
                        }

                        containingType = containingType.ContainingType;
                    }
                }

                return type;
            }

            return null;
        }
    }
}
