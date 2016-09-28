namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class TypeSyntaxExt
    {
        internal static bool IsDependencyProperty(this TypeSyntax typeSyntax)
        {
            return (typeSyntax as IdentifierNameSyntax)?.Identifier.ValueText == Names.DependencyProperty;
        }
    }
}