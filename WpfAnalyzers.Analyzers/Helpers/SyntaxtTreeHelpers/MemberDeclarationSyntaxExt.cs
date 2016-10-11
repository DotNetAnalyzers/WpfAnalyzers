namespace WpfAnalyzers
{
    using System;

    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class MemberDeclarationSyntaxExt
    {
        [Obsolete("Don't use this")]
        internal static TypeDeclarationSyntax DeclaringType(this MemberDeclarationSyntax member)
        {
            return member.Parent as TypeDeclarationSyntax;
        }
    }
}