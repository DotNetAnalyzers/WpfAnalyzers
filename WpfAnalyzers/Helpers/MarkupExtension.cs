namespace WpfAnalyzers;

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
            convertMethod is { ReturnType: PredefinedTypeSyntax { Keyword.ValueText: "object" }, ParameterList.Parameters.Count: 1 })
        {
            using var walker = ReturnValueWalker.Borrow(convertMethod);
            using var returnTypes = PooledSet<ITypeSymbol>.Borrow();
            returnTypes.UnionWith(walker.ReturnValues.Select(x => semanticModel.GetType(x, cancellationToken)).Where(x => x is { })!);
            return returnTypes.TrySingle<ITypeSymbol>(out returnType);
        }

        return false;
    }
}
