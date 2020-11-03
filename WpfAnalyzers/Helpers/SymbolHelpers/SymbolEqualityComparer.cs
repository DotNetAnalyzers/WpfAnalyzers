namespace WpfAnalyzers
{
    using System;

    using Microsoft.CodeAnalysis;

    internal static class SymbolEqualityComparer
    {
        /// <summary>
        /// Not sure why this is needed.
        /// Can't be named Equals as RS1024 will nag when using it if so.
        /// </summary>
        internal static bool Equal(ITypeSymbol x, ITypeSymbol y)
        {
            if (x.IsReferenceType)
            {
                return Microsoft.CodeAnalysis.SymbolEqualityComparer.IncludeNullability.Equals(x, y);
            }

            return Microsoft.CodeAnalysis.SymbolEqualityComparer.Default.Equals(x, y);
        }

        [Obsolete("Never use this.", error: true)]
        //// ReSharper disable once UnusedMember.Global
        internal static new bool Equals(object? x, object? y) => throw new InvalidOperationException("Never use this.");
    }
}
