namespace WpfAnalyzers
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// The safe versions handle situations like partial classes when the node is not in the same syntax tree.
    /// </summary>
    internal static partial class SemanticModelExt
    {
        internal static bool TryGetSymbol<TSymbol>(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken, out TSymbol symbol)
            where TSymbol : class, ISymbol
        {
            symbol = GetSymbolSafe(semanticModel, node, cancellationToken) as TSymbol ??
                     GetDeclaredSymbolSafe(semanticModel, node, cancellationToken) as TSymbol;
            return symbol != null;
        }

        /// <summary>
        /// Same as SemanticModel.GetSymbolInfo().Symbol but works when <paramref name="node"/> is not in the syntax tree.
        /// Unwraps the await
        /// </summary>
        /// <param name="semanticModel">The <see cref="SemanticModel"/></param>
        /// <param name="node">The <see cref="SyntaxNode"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>An <see cref="ISymbol"/> or null</returns>
        internal static ISymbol GetSymbolSafe(this SemanticModel semanticModel, AwaitExpressionSyntax node, CancellationToken cancellationToken)
        {
            return semanticModel.GetSymbolSafe(node.Expression, cancellationToken);
        }

        /// <summary>
        /// Same as SemanticModel.GetSymbolInfo().Symbol but works when <paramref name="node"/> is not in the syntax tree.
        /// </summary>
        /// <param name="semanticModel">The <see cref="SemanticModel"/></param>
        /// <param name="node">The <see cref="SyntaxNode"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>An <see cref="ISymbol"/> or null</returns>
        internal static IMethodSymbol GetSymbolSafe(this SemanticModel semanticModel, ConstructorInitializerSyntax node, CancellationToken cancellationToken)
        {
            return (IMethodSymbol)semanticModel.GetSymbolSafe((SyntaxNode)node, cancellationToken);
        }

        /// <summary>
        /// Same as SemanticModel.GetSymbolInfo().Symbol but works when <paramref name="node"/> is not in the syntax tree.
        /// </summary>
        /// <param name="semanticModel">The <see cref="SemanticModel"/></param>
        /// <param name="node">The <see cref="SyntaxNode"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>An <see cref="ISymbol"/> or null</returns>
        internal static IMethodSymbol GetSymbolSafe(this SemanticModel semanticModel, ObjectCreationExpressionSyntax node, CancellationToken cancellationToken)
        {
            return (IMethodSymbol)semanticModel.GetSymbolSafe((SyntaxNode)node, cancellationToken);
        }

        /// <summary>
        /// Same as SemanticModel.GetSymbolInfo().Symbol but works when <paramref name="node"/> is not in the syntax tree.
        /// </summary>
        /// <param name="semanticModel">The <see cref="SemanticModel"/></param>
        /// <param name="node">The <see cref="SyntaxNode"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>An <see cref="ISymbol"/> or null</returns>
        internal static ISymbol GetSymbolSafe(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
        {
            if (node is AwaitExpressionSyntax awaitExpression)
            {
                return GetSymbolSafe(semanticModel, awaitExpression, cancellationToken);
            }

            return semanticModel.SemanticModelFor(node)
                                ?.GetSymbolInfo(node, cancellationToken)
                                .Symbol;
        }

        internal static Optional<object> GetConstantValueSafe(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
        {
            return semanticModel.SemanticModelFor(node)
                                ?.GetConstantValue(node, cancellationToken) ?? default(Optional<object>);
        }

        internal static bool TryGetConstantValue<T>(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken, out T value)
        {
            var optional = GetConstantValueSafe(semanticModel, node, cancellationToken);
            if (optional.HasValue)
            {
                value = (T)optional.Value;
                return true;
            }

            value = default(T);
            return false;
        }

        internal static TypeInfo GetTypeInfoSafe(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
        {
            return semanticModel.SemanticModelFor(node)
                                ?.GetTypeInfo(node, cancellationToken) ?? default(TypeInfo);
        }

        /// <summary>
        /// Gets the semantic model for <paramref name="expression"/>
        /// This can be needed for partial classes.
        /// </summary>
        /// <param name="semanticModel">The semantic model.</param>
        /// <param name="expression">The expression.</param>
        /// <returns>The semantic model that corresponds to <paramref name="expression"/></returns>
        internal static SemanticModel SemanticModelFor(this SemanticModel semanticModel, SyntaxNode expression)
        {
            if (semanticModel == null ||
                expression == null ||
                expression.IsMissing)
            {
                return null;
            }

            if (ReferenceEquals(semanticModel.SyntaxTree, expression.SyntaxTree))
            {
                return semanticModel;
            }

            return Cache.GetOrAdd(expression.SyntaxTree, GetSemanticModel);

            SemanticModel GetSemanticModel(SyntaxTree syntaxTree)
            {
                if (semanticModel.Compilation.ContainsSyntaxTree(expression.SyntaxTree))
                {
                    return semanticModel.Compilation.GetSemanticModel(expression.SyntaxTree);
                }

                foreach (var metadataReference in semanticModel.Compilation.References)
                {
                    if (metadataReference is CompilationReference compilationReference)
                    {
                        if (compilationReference.Compilation.ContainsSyntaxTree(expression.SyntaxTree))
                        {
                            return compilationReference.Compilation.GetSemanticModel(expression.SyntaxTree);
                        }
                    }
                }

                return null;
            }
        }
    }
}
