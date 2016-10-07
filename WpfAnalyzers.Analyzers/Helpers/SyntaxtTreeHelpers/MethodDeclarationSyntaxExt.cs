namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class MethodDeclarationSyntaxExt
    {
        internal static string Name(this MethodDeclarationSyntax method)
        {
            return method?.Identifier.ValueText;
        }
    }
}