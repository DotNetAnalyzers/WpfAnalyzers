namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal static class TypeSymbolExt
    {
        internal static IEnumerable<ITypeSymbol> RecursiveBaseTypes(this ITypeSymbol type)
        {
            while (type is { })
            {
                foreach (var @interface in type.AllInterfaces)
                {
                    yield return @interface;
                }

                type = type.BaseType;
                if (type is { })
                {
                    yield return type;
                }
            }
        }
    }
}
