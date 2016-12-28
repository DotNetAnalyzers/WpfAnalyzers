namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal struct FixedTypes
    {
        internal readonly TypeDeclarationSyntax OldType;
        internal readonly TypeDeclarationSyntax FixedType;

        public FixedTypes(TypeDeclarationSyntax oldType, TypeDeclarationSyntax fixedType)
        {
            this.OldType = oldType;
            this.FixedType = fixedType;
        }
    }
}
