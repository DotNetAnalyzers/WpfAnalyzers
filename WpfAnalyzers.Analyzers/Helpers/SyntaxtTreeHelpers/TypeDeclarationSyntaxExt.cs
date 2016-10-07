namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class TypeDeclarationSyntaxExt
    {
        internal static FieldDeclarationSyntax Field(this TypeDeclarationSyntax type, string name)
        {
            if (type == null || type.IsMissing || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            foreach (var member in type.Members)
            {
                var field = member as FieldDeclarationSyntax;
                if (field.Name() == name)
                {
                    return field;
                }
            }

            return null;
        }

        internal static FieldDeclarationSyntax Field(this TypeDeclarationSyntax type, IdentifierNameSyntax name)
        {
            return type?.Field(name?.Identifier.ValueText);
        }
    }
}