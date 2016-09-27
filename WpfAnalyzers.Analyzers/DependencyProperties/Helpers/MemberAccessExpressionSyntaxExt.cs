namespace WpfAnalyzers.DependencyProperties
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class MemberAccessExpressionSyntaxExt
    {
        internal static bool IsDependencyPropertyRegister(this MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.IsDependencyPropertyCall("Register");
        }

        internal static bool IsDependencyPropertyRegisterAttached(this MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.IsDependencyPropertyCall("RegisterAttached");
        }

        internal static bool IsDependencyPropertyRegisterReadOnly(this MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.IsDependencyPropertyCall("RegisterReadOnly");
        }

        internal static bool IsDependencyPropertyRegisterAttachedReadOnly(this MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.IsDependencyPropertyCall("RegisterAttachedReadOnly");
        }

        internal static bool IsDependencyPropertyKeyProperty(this MemberAccessExpressionSyntax memberAccess)
        {
            if (memberAccess == null || memberAccess.IsMissing)
            {
                return false;
            }

            return memberAccess.Name?.Identifier.ValueText == "DependencyProperty";
        }

        private static bool IsDependencyPropertyCall(this MemberAccessExpressionSyntax memberAccess, string name)
        {
            if (memberAccess == null || memberAccess.IsMissing)
            {
                return false;
            }

            if ((memberAccess.Expression as IdentifierNameSyntax)?.Identifier.ValueText != Names.DependencyProperty)
            {
                return false;
            }

            return memberAccess.Name?.Identifier.ValueText == name;
        }
    }
}