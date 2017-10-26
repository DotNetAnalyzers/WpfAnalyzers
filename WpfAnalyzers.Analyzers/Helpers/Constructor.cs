namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Constructor
    {
        internal static bool TryGet(ObjectCreationExpressionSyntax objectCreation, QualifiedType qualifiedType, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol ctor)
        {
            if (objectCreation.Type is IdentifierNameSyntax typeName &&
                (typeName.Identifier.ValueText == qualifiedType.Type ||
                 AliasWalker.Contains(objectCreation.SyntaxTree, typeName.Identifier.ValueText)))
            {
                ctor = semanticModel.GetSymbolSafe(objectCreation, cancellationToken) as IMethodSymbol;
                return ctor?.ContainingType == qualifiedType;
            }

            if (objectCreation.Type is QualifiedNameSyntax qualifiedName &&
                qualifiedName.Right.Identifier.ValueText == qualifiedType.Type)
            {
                ctor = semanticModel.GetSymbolSafe(objectCreation, cancellationToken) as IMethodSymbol;
                return ctor?.ContainingType == qualifiedType;
            }

            ctor = null;
            return false;
        }
    }
}