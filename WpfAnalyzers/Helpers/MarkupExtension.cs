namespace WpfAnalyzers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class MarkupExtension
    {
        internal static bool TryGetReturnType(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ITypeSymbol? returnType)
        {
            returnType = null;
            if (classDeclaration.TryFindMethod("ProvideValue", out var convertMethod) &&
                convertMethod.ReturnType is PredefinedTypeSyntax predefinedType &&
                predefinedType.Keyword.ValueText == "object" &&
                convertMethod.ParameterList is { } &&
                convertMethod.ParameterList.Parameters.Count == 1)
            {
                using var walker = ReturnValueWalker.Borrow(convertMethod);
                using var returnTypes = PooledSet<ITypeSymbol>.Borrow();
                returnTypes.UnionWith(walker.ReturnValues.Select(x => semanticModel.GetTypeInfoSafe(x, cancellationToken).Type));
                return returnTypes.TrySingle<ITypeSymbol>(out returnType);
            }

            return false;
        }
    }
}
