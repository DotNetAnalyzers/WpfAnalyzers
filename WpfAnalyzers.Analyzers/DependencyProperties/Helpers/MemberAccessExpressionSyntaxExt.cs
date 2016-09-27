namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class MemberAccessExpressionSyntaxExt
    {
        internal static bool IsDependencyPropertyRegister(this MemberAccessExpressionSyntax memberAccess)
        {
            if (memberAccess == null || memberAccess.IsMissing)
            {
                return false;
            }

            if ((memberAccess.Expression as IdentifierNameSyntax)?.Identifier.ValueText != Names.DependencyProperty)
            {
                return false;
            }

            if (memberAccess.Name.Identifier.ValueText != "Register")
            {
                return false;
            }

            return true;
        }
    }
}