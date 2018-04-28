namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal static class TypeSymbolExt
    {
        internal static IEnumerable<ITypeSymbol> RecursiveBaseTypes(this ITypeSymbol type)
        {
            while (type != null)
            {
                foreach (var @interface in type.AllInterfaces)
                {
                    yield return @interface;
                }

                type = type.BaseType;
                if (type != null)
                {
                    yield return type;
                }
            }
        }
    }
}
