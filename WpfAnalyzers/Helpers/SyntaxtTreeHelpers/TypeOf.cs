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
            if (semanticModel.TryGetType(expression.Type, cancellationToken, out type) &&
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
                            type = containingType.TypeArguments[index];
                            return true;
                        }
                    }

                    containingType = containingType.ContainingType;
                }
            }

            return type is { };
        }
    }
}
