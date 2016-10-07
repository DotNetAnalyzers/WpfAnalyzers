namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyDeclarationSyntaxExt
    {
        internal static string Name(this PropertyDeclarationSyntax property)
        {
            return property?.Identifier.ValueText;
        }

        internal static bool TryGetGetAccessorDeclaration(this PropertyDeclarationSyntax property, out AccessorDeclarationSyntax result)
        {
            return TryGetAccessorDeclaration(property, SyntaxKind.GetAccessorDeclaration, out result);
        }

        internal static bool TryGetSetAccessorDeclaration(this PropertyDeclarationSyntax property, out AccessorDeclarationSyntax result)
        {
            return TryGetAccessorDeclaration(property, SyntaxKind.SetAccessorDeclaration, out result);
        }

        internal static bool TryGetAccessorDeclaration(this PropertyDeclarationSyntax property, SyntaxKind kind, out AccessorDeclarationSyntax result)
        {
            result = null;
            var accessors = property?.AccessorList?.Accessors;
            if (accessors == null)
            {
                return false;
            }

            foreach (var accessor in accessors.Value)
            {
                if (accessor.IsKind(kind))
                {
                    result = accessor;
                    return true;
                }
            }

            return false;
        }
    }
}