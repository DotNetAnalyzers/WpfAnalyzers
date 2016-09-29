namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class MemberDeclarationSyntaxExt
    {
        internal static TypeDeclarationSyntax DeclaringType(this MemberDeclarationSyntax member)
        {
            return member.Parent as TypeDeclarationSyntax;
        }
    }
}