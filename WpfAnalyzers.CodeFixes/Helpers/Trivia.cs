namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    internal static class Trivia
    {
        internal static SyntaxNode WithTriviaFrom(this SyntaxNode node, SyntaxNode other)
        {
            return node.WithLeadingTriviaFrom(other)
                       .WithTrailingTriviaFrom(other);
        }

        internal static SyntaxNode WithLeadingTriviaFrom(this SyntaxNode node, SyntaxNode other)
        {
            return other.HasLeadingTrivia
                ? node.WithLeadingTrivia(other.GetLeadingTrivia())
                : node;
        }

        internal static SyntaxNode WithTrailingTriviaFrom(this SyntaxNode node, SyntaxNode other)
        {
            return other.HasTrailingTrivia
                ? node.WithTrailingTrivia(other.GetTrailingTrivia())
                : node;
        }

        internal static T WithLeadingElasticLineFeed<T>(this T node)
            where T : SyntaxNode
        {
            if (node.HasLeadingTrivia)
            {
                return node.WithLeadingTrivia(
                    node.GetLeadingTrivia()
                        .Insert(0, SyntaxFactory.ElasticLineFeed));
            }

            return node.WithLeadingTrivia(SyntaxFactory.ElasticLineFeed);
        }

        internal static T WithLeadingLineFeed<T>(this T node)
            where T : SyntaxNode
        {
            if (node.HasLeadingTrivia)
            {
                return node.WithLeadingTrivia(
                    node.GetLeadingTrivia()
                        .Insert(0, SyntaxFactory.LineFeed));
            }

            return node.WithLeadingTrivia(SyntaxFactory.LineFeed);
        }

        internal static T WithTrailingElasticLineFeed<T>(this T node)
            where T : SyntaxNode
        {
            if (node.HasTrailingTrivia)
            {
                return node.WithTrailingTrivia(
                    node.GetTrailingTrivia()
                        .Add(SyntaxFactory.ElasticLineFeed));
            }

            return node.WithTrailingTrivia(SyntaxFactory.ElasticLineFeed);
        }

        internal static T WithTrailingLineFeed<T>(this T node)
            where T : SyntaxNode
        {
            if (node.HasTrailingTrivia)
            {
                return node.WithTrailingTrivia(
                    node.GetTrailingTrivia()
                        .Add(SyntaxFactory.LineFeed));
            }

            return node.WithTrailingTrivia(SyntaxFactory.LineFeed);
        }
    }
}