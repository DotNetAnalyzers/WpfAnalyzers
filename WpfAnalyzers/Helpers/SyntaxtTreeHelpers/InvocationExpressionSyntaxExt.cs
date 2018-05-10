namespace WpfAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class InvocationExpressionSyntaxExt
    {
        internal static bool TrySingleArgument(this InvocationExpressionSyntax invocation, out ArgumentSyntax result)
        {
            result = null;
            return invocation?.ArgumentList?.Arguments.TrySingle(out result) == true;
        }

        internal static bool TryGetArgumentAtIndex(this InvocationExpressionSyntax invocation, int index, out ArgumentSyntax result)
        {
            result = null;
            return invocation?.ArgumentList?.Arguments.TryElementAt(index, out result) == true;
        }
    }
}
