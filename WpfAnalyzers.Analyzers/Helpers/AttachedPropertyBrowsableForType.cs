namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class AttachedPropertyBrowsableForType
    {
        internal static bool TryGetParameterType(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol type)
        {
            type = null;
            if (methodDeclaration == null ||
                methodDeclaration.ParameterList == null ||
                methodDeclaration.ParameterList.Parameters.Count != 1)
            {
                return false;
            }

            type = semanticModel.GetDeclaredSymbolSafe(methodDeclaration, cancellationToken)
                                .Parameters[0]
                                .Type;
            return true;
        }
    }
}
