namespace WpfAnalyzers
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// The safe versions handle situations like partial classes when the node is not in the same syntax tree.
    /// </summary>
    internal static class SemanticModelExt
    {
        internal static ISymbol GetSymbolSafe(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
        {
            if (node == null)
            {
                return null;
            }

            var semanticModelFor = semanticModel.SemanticModelFor(node);
            if (semanticModelFor != null)
            {
                return semanticModelFor.GetSymbolInfo(node, cancellationToken).Symbol;
            }

            return semanticModel?.GetSymbolInfo(node, cancellationToken).Symbol;
        }

        internal static IFieldSymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, FieldDeclarationSyntax node, CancellationToken cancellationToken)
        {
            return (IFieldSymbol)GetDeclaredSymbolSafe(semanticModel, (SyntaxNode)node, cancellationToken);
        }

        internal static IMethodSymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, ConstructorDeclarationSyntax node, CancellationToken cancellationToken)
        {
            return (IMethodSymbol)GetDeclaredSymbolSafe(semanticModel, (SyntaxNode)node, cancellationToken);
        }

        internal static IPropertySymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, PropertyDeclarationSyntax node, CancellationToken cancellationToken)
        {
            return (IPropertySymbol)GetDeclaredSymbolSafe(semanticModel, (SyntaxNode)node, cancellationToken);
        }

        internal static IMethodSymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, MethodDeclarationSyntax node, CancellationToken cancellationToken)
        {
            return (IMethodSymbol)GetDeclaredSymbolSafe(semanticModel, (SyntaxNode)node, cancellationToken);
        }

        internal static ITypeSymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, TypeDeclarationSyntax node, CancellationToken cancellationToken)
        {
            return (ITypeSymbol)GetDeclaredSymbolSafe(semanticModel, (SyntaxNode)node, cancellationToken);
        }

        internal static ISymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
        {
            if (node == null)
            {
                return null;
            }

            var semanticModelFor = semanticModel.SemanticModelFor(node);
            if (semanticModelFor != null)
            {
                return semanticModelFor.GetDeclaredSymbol(node, cancellationToken);
            }

            return semanticModel?.GetDeclaredSymbol(node, cancellationToken);
        }

        internal static Optional<object> GetConstantValueSafe(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
        {
            if (node == null)
            {
                return default(Optional<object>);
            }

            var semanticModelFor = semanticModel.SemanticModelFor(node);
            if (semanticModelFor != null)
            {
                return semanticModelFor.GetConstantValue(node, cancellationToken);
            }

            return semanticModel?.GetConstantValue(node, cancellationToken) ?? default(Optional<object>);
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
            if (node == null)
            {
                return default(TypeInfo);
            }

            var semanticModelFor = semanticModel.SemanticModelFor(node);
            if (semanticModelFor != null)
            {
                return semanticModelFor.GetTypeInfo(node, cancellationToken);
            }

            return semanticModel?.GetTypeInfo(node, cancellationToken) ?? default(TypeInfo);
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
            if (semanticModel == null || expression == null)
            {
                return null;
            }

            return ReferenceEquals(semanticModel.SyntaxTree, expression.SyntaxTree)
                ? semanticModel
                : semanticModel.Compilation.GetSemanticModel(expression.SyntaxTree);
        }
    }
}