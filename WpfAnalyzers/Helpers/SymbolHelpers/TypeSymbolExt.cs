namespace WpfAnalyzers;

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

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            type = type.BaseType;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            if (type is { })
            {
                yield return type;
            }
        }
    }
}
