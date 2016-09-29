namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class TypeHelper
    {
        internal static bool IsSameType(ITypeSymbol first, ITypeSymbol other)
        {
            if (first == null || other == null)
            {
                return false;
            }

            return first.Equals(other);
        }
    }
}
