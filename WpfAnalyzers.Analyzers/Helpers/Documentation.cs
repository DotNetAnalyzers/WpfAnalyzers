namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Documentation
    {
        internal static bool HasDocumentation(this SyntaxNode node)
        {
            return GetDocumentationCommentTrivia(node) != null;
        }

        private static DocumentationCommentTriviaSyntax GetDocumentationCommentTrivia(SyntaxNode node)
        {
            if (node == null ||
                !node.HasLeadingTrivia)
            {
                return null;
            }

            foreach (var leadingTrivia in node.GetLeadingTrivia())
            {
                if (leadingTrivia.GetStructure() is DocumentationCommentTriviaSyntax structure)
                {
                    return structure;
                }
            }

            return null;
        }
    }
}
