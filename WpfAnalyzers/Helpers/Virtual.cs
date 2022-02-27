namespace WpfAnalyzers;

using Microsoft.CodeAnalysis;

internal static class Virtual
{
    internal static bool HasVirtualOrAbstractOrProtectedMembers(ITypeSymbol type)
    {
        if (type is null ||
            type.IsStatic ||
            type.IsSealed)
        {
            return false;
        }

        foreach (var member in type.GetMembers())
        {
            if (member.IsAbstract ||
                member.IsVirtual ||
                member.DeclaredAccessibility == Accessibility.Protected)
            {
                return true;
            }
        }

        return false;
    }
}
