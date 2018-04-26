namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static partial class SemanticModelExt
    {
        /// <summary>
        /// Same as SemanticModel.GetDeclaredSymbol but works when <paramref name="node"/> is not in the syntax tree.
        /// </summary>
        /// <param name="semanticModel">The <see cref="SemanticModel"/></param>
        /// <param name="node">The <see cref="SyntaxNode"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>An <see cref="IFieldSymbol"/> or null</returns>
        internal static IFieldSymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, FieldDeclarationSyntax node, CancellationToken cancellationToken)
        {
            if (node?.Declaration == null)
            {
                return null;
            }

            if (node.Declaration.Variables.TrySingle(out var variable))
            {
                return (IFieldSymbol)semanticModel.SemanticModelFor(node)
                                                  ?.GetDeclaredSymbol(variable, cancellationToken);
            }

            return null;
        }

        /// <summary>
        /// Same as SemanticModel.GetDeclaredSymbol but works when <paramref name="node"/> is not in the syntax tree.
        /// </summary>
        /// <param name="semanticModel">The <see cref="SemanticModel"/></param>
        /// <param name="node">The <see cref="SyntaxNode"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>An <see cref="IMethodSymbol"/> or null</returns>
        internal static IMethodSymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, ConstructorDeclarationSyntax node, CancellationToken cancellationToken)
        {
            return (IMethodSymbol)semanticModel.SemanticModelFor(node)
                                               ?.GetDeclaredSymbol(node, cancellationToken);
        }

        /// <summary>
        /// Same as SemanticModel.GetDeclaredSymbol but works when <paramref name="node"/> is not in the syntax tree.
        /// </summary>
        /// <param name="semanticModel">The <see cref="SemanticModel"/></param>
        /// <param name="node">The <see cref="SyntaxNode"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>An <see cref="IPropertySymbol"/> or null</returns>
        internal static IPropertySymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, PropertyDeclarationSyntax node, CancellationToken cancellationToken)
        {
            return (IPropertySymbol)semanticModel.SemanticModelFor(node)
                                                 ?.GetDeclaredSymbol(node, cancellationToken);
        }

        /// <summary>
        /// Same as SemanticModel.GetDeclaredSymbol but works when <paramref name="node"/> is not in the syntax tree.
        /// </summary>
        /// <param name="semanticModel">The <see cref="SemanticModel"/></param>
        /// <param name="node">The <see cref="SyntaxNode"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>An <see cref="IPropertySymbol"/> or null</returns>
        internal static IPropertySymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, IndexerDeclarationSyntax node, CancellationToken cancellationToken)
        {
            return (IPropertySymbol)semanticModel.SemanticModelFor(node)
                                                 ?.GetDeclaredSymbol(node, cancellationToken);
        }

        /// <summary>
        /// Same as SemanticModel.GetDeclaredSymbol but works when <paramref name="node"/> is not in the syntax tree.
        /// </summary>
        /// <param name="semanticModel">The <see cref="SemanticModel"/></param>
        /// <param name="node">The <see cref="SyntaxNode"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>An <see cref="IMethodSymbol"/> or null</returns>
        internal static IMethodSymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, MethodDeclarationSyntax node, CancellationToken cancellationToken)
        {
            return (IMethodSymbol)semanticModel.SemanticModelFor(node)
                                               ?.GetDeclaredSymbol(node, cancellationToken);
        }

        /// <summary>
        /// Same as SemanticModel.GetDeclaredSymbol but works when <paramref name="node"/> is not in the syntax tree.
        /// </summary>
        /// <param name="semanticModel">The <see cref="SemanticModel"/></param>
        /// <param name="node">The <see cref="SyntaxNode"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>An <see cref="ITypeSymbol"/> or null</returns>
        internal static ITypeSymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, TypeDeclarationSyntax node, CancellationToken cancellationToken)
        {
            return (ITypeSymbol)semanticModel.SemanticModelFor(node)
                                             ?.GetDeclaredSymbol(node, cancellationToken);
        }

        /// <summary>
        /// Same as SemanticModel.GetDeclaredSymbol but works when <paramref name="node"/> is not in the syntax tree.
        /// </summary>
        /// <param name="semanticModel">The <see cref="SemanticModel"/></param>
        /// <param name="node">The <see cref="SyntaxNode"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>An <see cref="IParameterSymbol"/> or null</returns>
        internal static IParameterSymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, ParameterSyntax node, CancellationToken cancellationToken)
        {
            return (IParameterSymbol)GetDeclaredSymbolSafe(semanticModel, (SyntaxNode)node, cancellationToken);
        }

        /// <summary>
        /// Same as SemanticModel.GetDeclaredSymbol but works when <paramref name="node"/> is not in the syntax tree.
        /// </summary>
        /// <param name="semanticModel">The <see cref="SemanticModel"/></param>
        /// <param name="node">The <see cref="SyntaxNode"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>An <see cref="ILocalSymbol"/> or null</returns>
        internal static ILocalSymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, VariableDeclarationSyntax node, CancellationToken cancellationToken)
        {
            if (node?.Variables == null)
            {
                return null;
            }

            if (node.Variables.TrySingle(out var variable))
            {
                return (ILocalSymbol)semanticModel.SemanticModelFor(node)
                                                  ?.GetDeclaredSymbol(variable, cancellationToken);
            }

            return null;
        }

        /// <summary>
        /// Same as SemanticModel.GetDeclaredSymbol but works when <paramref name="node"/> is not in the syntax tree.
        /// </summary>
        /// <param name="semanticModel">The <see cref="SemanticModel"/></param>
        /// <param name="node">The <see cref="SyntaxNode"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>An <see cref="ISymbol"/> or null</returns>
        internal static ISymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, BasePropertyDeclarationSyntax node, CancellationToken cancellationToken)
        {
            return semanticModel.SemanticModelFor(node)
                                ?.GetDeclaredSymbol(node, cancellationToken);
        }

        internal static ISymbol GetDeclaredSymbolSafe(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
        {
            if (node is FieldDeclarationSyntax fieldDeclaration)
            {
                return GetDeclaredSymbolSafe(semanticModel, fieldDeclaration, cancellationToken);
            }

            return semanticModel.SemanticModelFor(node)
                                ?.GetDeclaredSymbol(node, cancellationToken);
        }
    }
}
