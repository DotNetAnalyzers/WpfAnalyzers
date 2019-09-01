namespace WpfAnalyzers
{
    using System;
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    [Obsolete("Use Gu.Roslyn.Extensions.")]
    internal static class TriviaExt
    {
        /// <summary>
        /// Add leading line feed to <paramref name="node"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="SyntaxNode"/>.</typeparam>
        /// <param name="node">The <see cref="SyntaxNode"/>.</param>
        /// <returns><paramref name="node"/> with leading line feed.</returns>
        internal static T WithoutLeadingLineFeed<T>(this T node)
            where T : SyntaxNode
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (node.HasLeadingTrivia &&
                node.GetLeadingTrivia() is SyntaxTriviaList triviaList &&
                triviaList.TryFirst(out var first) &&
                first.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return node.WithLeadingTrivia(triviaList.Skip(1));
            }

            return node;
        }
    }
}
