namespace WpfAnalyzers
{
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class MarkupExtension
    {
        internal static bool TryGetReturnType(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol returnType)
        {
            returnType = null;
            if (classDeclaration.TryFindMethod("ProvideValue", out var convertMethod) &&
                convertMethod.ReturnType is PredefinedTypeSyntax predefinedType &&
                predefinedType.Keyword.ValueText == "object" &&
                convertMethod.ParameterList != null &&
                convertMethod.ParameterList.Parameters.Count == 1)
            {
                using (var walker = ReturnExpressionsWalker.Borrow(convertMethod))
                {
                    using (var returnTypes = PooledSet<ITypeSymbol>.Borrow())
                    {
                        returnTypes.UnionWith(walker.ReturnValues.Select(x => semanticModel.GetTypeInfoSafe(x, cancellationToken).Type));
                        return returnTypes.TryGetSingle(out returnType);
                    }
                }
            }

            return false;
        }
    }
}