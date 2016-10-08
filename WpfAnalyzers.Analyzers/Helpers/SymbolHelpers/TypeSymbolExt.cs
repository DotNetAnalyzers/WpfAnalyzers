namespace WpfAnalyzers.SymbolHelpers
{
    using Microsoft.CodeAnalysis;

    internal static class TypeSymbolExt
    {
        internal static bool IsSameType(this ITypeSymbol first, ITypeSymbol other)
        {
            if (first == null || other == null)
            {
                return false;
            }

            return first.Equals(other);
        }

        internal static bool IsAssignableToDependencyObject(this ITypeSymbol type)
        {
            while (type?.BaseType != null)
            {
                if (type.Name == Names.DependencyObject)
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }

        internal static bool IsAssignableTo(this ITypeSymbol type, ITypeSymbol other)
        {
            while (type?.BaseType != null)
            {
                if (IsSameType(type, other))
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}