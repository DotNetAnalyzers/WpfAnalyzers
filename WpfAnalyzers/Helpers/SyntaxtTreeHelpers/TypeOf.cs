namespace WpfAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class TypeOf
    {
        internal static bool TryGetType(TypeOfExpressionSyntax expression, INamedTypeSymbol containingType, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ITypeSymbol? type)
        {
            if (expression is { Type: { } typeSyntax })
            {
                type = TryGetType(typeSyntax, containingType, semanticModel, cancellationToken);
                return type is { };
            }

            type = null;
            return false;
        }

        internal static ITypeSymbol? TryGetType(TypeSyntax typeSyntax, INamedTypeSymbol containingType, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (semanticModel.TryGetType(typeSyntax, cancellationToken, out var type) &&
                type.Kind == SymbolKind.TypeParameter)
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
    }
}
