namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Constructor
    {
        internal static bool TryGet(ObjectCreationExpressionSyntax objectCreation, QualifiedType qualifiedType, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol ctor)
        {
            ctor = null;
            if (objectCreation.Type is SimpleNameSyntax simpleName)
            {
                if (simpleName.Identifier.ValueText == qualifiedType.Type ||
                    AliasWalker.Contains(objectCreation.SyntaxTree, simpleName.Identifier.ValueText))
                {
                    ctor = SemanticModelExt.GetSymbolSafe(semanticModel, objectCreation, cancellationToken);
                    return ctor?.ContainingType == qualifiedType;
                }

                return false;
            }

            if (objectCreation.Type is QualifiedNameSyntax qualifiedName)
            {
                var typeName = qualifiedName.Right.Identifier.ValueText;
                if (typeName == qualifiedType.Type ||
                    AliasWalker.Contains(objectCreation.SyntaxTree, typeName))
                {
                    ctor = SemanticModelExt.GetSymbolSafe(semanticModel, objectCreation, cancellationToken);
                    return ctor?.ContainingType == qualifiedType;
                }

                return false;
            }

            return false;
        }
    }
}
