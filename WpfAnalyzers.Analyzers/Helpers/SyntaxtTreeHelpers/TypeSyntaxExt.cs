namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class TypeSyntaxExt
    {
        internal static bool IsSameType(this TypeSyntax typeSyntax, QualifiedType qualifiedType, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (typeSyntax is SimpleNameSyntax simpleName)
            {
                if (simpleName.Identifier.ValueText == qualifiedType.Type ||
                    AliasWalker.Contains(typeSyntax.SyntaxTree, simpleName.Identifier.ValueText))
                {
                    return semanticModel.GetTypeInfoSafe(typeSyntax, cancellationToken).Type == qualifiedType;
                }

                return false;
            }

            if (typeSyntax is QualifiedNameSyntax qualifiedName)
            {
                var typeName = qualifiedName.Right.Identifier.ValueText;
                if (typeName == qualifiedType.Type ||
                    AliasWalker.Contains(typeSyntax.SyntaxTree, typeName))
                {
                    return semanticModel.GetTypeInfoSafe(typeSyntax, cancellationToken).Type == qualifiedType;
                }

                return false;
            }

            return semanticModel.GetTypeInfoSafe(typeSyntax, cancellationToken).Type == qualifiedType;
        }

        internal static bool IsVoid(this TypeSyntax type)
        {
            if (type is PredefinedTypeSyntax predefinedType)
            {
                return predefinedType.Keyword.ValueText == "void";
            }

            return false;
        }
    }
}