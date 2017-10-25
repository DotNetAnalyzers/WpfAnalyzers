﻿namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class SymbolExt
    {
        internal static bool IsEither<T1, T2>(this ISymbol symbol)
            where T1 : ISymbol
            where T2 : ISymbol
        {
            return symbol is T1 || symbol is T2;
        }

        internal static bool TryGetSingleDeclaration(this IFieldSymbol symbol, CancellationToken cancellationToken, out FieldDeclarationSyntax declaration)
        {
            return TryGetSingleDeclaration<FieldDeclarationSyntax>(symbol, cancellationToken, out declaration);
        }

        internal static bool TryGetSingleDeclaration(this IPropertySymbol symbol, CancellationToken cancellationToken, out PropertyDeclarationSyntax declaration)
        {
            return TryGetSingleDeclaration<PropertyDeclarationSyntax>(symbol, cancellationToken, out declaration);
        }

        internal static bool TryGetSingleDeclaration(this IMethodSymbol symbol, CancellationToken cancellationToken, out MethodDeclarationSyntax declaration)
        {
            return TryGetSingleDeclaration<MethodDeclarationSyntax>(symbol, cancellationToken, out declaration);
        }

        internal static bool TryGetSingleDeclaration<T>(this ISymbol symbol, CancellationToken cancellationToken, out T declaration)
            where T : SyntaxNode
        {
            declaration = null;
            if (symbol == null)
            {
                return false;
            }

            if (symbol.DeclaringSyntaxReferences.TryGetSingle(out var syntaxReference))
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