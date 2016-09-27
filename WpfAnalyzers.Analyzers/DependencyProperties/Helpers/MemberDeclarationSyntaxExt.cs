namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class MemberDeclarationSyntaxExt
    {
        internal static ClassDeclarationSyntax Class(this MemberDeclarationSyntax member)
        {
            return (ClassDeclarationSyntax) member.Parent;
        }
    }
}