namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ClassDeclarationSyntaxExt
    {
        internal static string Name(this ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration?.Identifier.Text;
        }

        internal static FieldDeclarationSyntax FieldDeclaration(this ClassDeclarationSyntax classSyntax, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            foreach (var member in classSyntax.Members)
            {
                var field = member as FieldDeclarationSyntax;
                if (field.Name() == name)
                {
                    return field;
                }
            }

            return null;
        }
    }
}