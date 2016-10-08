namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class AccessorDeclarationSyntaxExt
    {
        internal static TypeDeclarationSyntax DeclaringType(this AccessorDeclarationSyntax accessor)
        {
            var accessors = (AccessorListSyntax)accessor?.Parent;
            var property = (PropertyDeclarationSyntax)accessors?.Parent;
            return property.DeclaringType();
        }
    }
}