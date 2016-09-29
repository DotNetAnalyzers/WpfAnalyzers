namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class TypeSyntaxExt
    {
        internal static string Name(this TypeSyntax type)
        {
            var identifier = type as IdentifierNameSyntax;
            if (identifier != null)
            {
                return identifier.Identifier.ValueText;
            }

            var predefinedType = type as PredefinedTypeSyntax;
            if (predefinedType != null)
            {
                return predefinedType.Keyword.ValueText;
            }

            return type?.ToString();
        }

        internal static bool IsVoid(this TypeSyntax type)
        {
            var predefinedType = type as PredefinedTypeSyntax;
            if (predefinedType == null)
            {
                return false;
            }

            return predefinedType.Keyword.ValueText == "void";
        }
    }
}