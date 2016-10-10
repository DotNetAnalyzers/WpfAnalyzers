namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class SemanticModelExt
    {
        /// <summary>
        /// Gets the semantic model for <paramref name="expression"/>
        /// This can be needed for partial classes.
        /// </summary>
        /// <param name="semanticModel">The semantic model.</param>
        /// <param name="expression">The expression.</param>
        /// <returns>The semantic model that corresponds to <paramref name="expression"/></returns>
        internal static SemanticModel SemanticModelFor(this SemanticModel semanticModel, SyntaxNode expression)
        {
            if (ReferenceEquals(semanticModel.SyntaxTree, expression.SyntaxTree))
            {
                return semanticModel;
            }

            return semanticModel.Compilation.GetSemanticModel(expression.SyntaxTree);
        }
    }
}