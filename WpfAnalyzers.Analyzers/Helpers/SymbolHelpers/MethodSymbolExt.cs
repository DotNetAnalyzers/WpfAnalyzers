namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;

    using Microsoft.CodeAnalysis;

    internal static class MethodSymbolExt
    {
        internal static bool TryGetSingleDeclaration<T>(this IMethodSymbol symbol, CancellationToken cancellationToken, out SyntaxNode declaration)
        {
            declaration = null;
            if (symbol == null)
            {
                return false;
            }

            SyntaxReference syntaxReference;
            if (symbol.DeclaringSyntaxReferences.TryGetSingle(out syntaxReference))
            {
                declaration = syntaxReference.GetSyntax(cancellationToken);
                return declaration != null;
            }

            return false;
        }

        internal static IEnumerable<SyntaxNode> Declarations(this IMethodSymbol symbol, CancellationToken cancellationToken)
        {
            foreach (var syntaxReference in symbol.DeclaringSyntaxReferences)
            {
                yield return syntaxReference.GetSyntax(cancellationToken);
            }
        }
    }
}