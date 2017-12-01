namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ObjectCreationExpressionSyntaxExt
    {
        internal static bool TryGetSingleArgument(this ObjectCreationExpressionSyntax objectCreation, out ArgumentSyntax result)
        {
            result = null;
            return objectCreation?.ArgumentList?.Arguments.TryGetSingle(out result) == true;
        }
    }
}