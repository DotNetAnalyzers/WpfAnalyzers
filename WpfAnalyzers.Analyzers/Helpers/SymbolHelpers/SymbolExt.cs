namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;

    using Microsoft.CodeAnalysis;

    internal static class SymbolExt
    {
        internal static bool IsEither<T1, T2>(this ISymbol symbol)
            where T1 : ISymbol
            where T2 : ISymbol
        {
            return symbol is T1 || symbol is T2;
        }

        internal static bool TryGetSingleDeclaration<T>(this ISymbol symbol, CancellationToken cancellationToken, out T declaration)
            where T : SyntaxNode
        {
            declaration = null;
            if (symbol == null)
            {
                return false;
            }

            if (symbol.DeclaringSyntaxReferences.TryGetSingle(out SyntaxReference syntaxReference))
            {
                declaration = syntaxReference.GetSyntax(cancellationToken) as T;
                return declaration != null;
            }

            return false;
        }

        internal static IEnumerable<SyntaxNode> Declarations(this ISymbol symbol, CancellationToken cancellationToken)
        {
            foreach (var syntaxReference in symbol.DeclaringSyntaxReferences)
            {
                yield return syntaxReference.GetSyntax(cancellationToken);
            }
        }
    }
}