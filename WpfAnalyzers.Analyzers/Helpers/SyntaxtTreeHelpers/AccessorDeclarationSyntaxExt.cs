namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class AccessorDeclarationSyntaxExt
    {
        internal static bool TryGetControlFlow(this AccessorDeclarationSyntax accessor, SemanticModel semanticModel, out ControlFlowAnalysis result)
        {
            if (accessor?.Body == null)
            {
                result = null;
                return false;
            }

            result = semanticModel.SemanticModelFor(accessor.Body)
               .AnalyzeControlFlow(accessor.Body);

            return result != null && result.Succeeded;
        }

        internal static bool TryGetDataFlow(this AccessorDeclarationSyntax accessor, SemanticModel semanticModel, out DataFlowAnalysis result)
        {
            if (accessor?.Body == null)
            {
                result = null;
                return false;
            }

            result = semanticModel.SemanticModelFor(accessor.Body)
                                  .AnalyzeDataFlow(accessor.Body);

            return result != null && result.Succeeded;
        }
    }
}