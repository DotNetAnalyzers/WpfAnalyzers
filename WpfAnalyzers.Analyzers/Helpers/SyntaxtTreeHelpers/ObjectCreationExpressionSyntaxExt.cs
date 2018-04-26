namespace WpfAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ObjectCreationExpressionSyntaxExt
    {
        internal static bool TrySingleArgument(this ObjectCreationExpressionSyntax objectCreation, out ArgumentSyntax result)
        {
            result = null;
            return objectCreation?.ArgumentList?.Arguments.TrySingle(out result) == true;
        }
    }
}
