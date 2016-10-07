namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class MemberAccessExpressionSyntaxExt
    {
        internal static bool IsDependencyPropertyRegister(this MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.IsDependencyPropertyCall(Names.Register);
        }

        internal static bool IsDependencyPropertyRegisterAttached(this MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.IsDependencyPropertyCall(Names.RegisterAttached);
        }

        internal static bool IsDependencyPropertyRegisterReadOnly(this MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.IsDependencyPropertyCall(Names.RegisterReadOnly);
        }

        internal static bool IsDependencyPropertyRegisterAttachedReadOnly(this MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.IsDependencyPropertyCall(Names.RegisterAttachedReadOnly);
        }

        internal static bool IsDependencyPropertyAddOwner(this MemberAccessExpressionSyntax memberAccess)
        {
            if (memberAccess == null || memberAccess.IsMissing)
            {
                return false;
            }

            return memberAccess.Name?.Identifier.ValueText == Names.AddOwner;
        }

        internal static bool IsDependencyPropertyKeyProperty(this MemberAccessExpressionSyntax memberAccess)
        {
            if (memberAccess == null || memberAccess.IsMissing)
            {
                return false;
            }

            return memberAccess.Name?.Identifier.ValueText == Names.DependencyProperty;
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