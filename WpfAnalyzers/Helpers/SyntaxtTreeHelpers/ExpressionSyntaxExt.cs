namespace WpfAnalyzers;

using Gu.Roslyn.AnalyzerExtensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class ExpressionSyntaxExt
{
    internal static bool IsNameof(this ExpressionSyntax candidate) => candidate is InvocationExpressionSyntax invocation &&
                                                                      invocation.IsNameOf();
}
