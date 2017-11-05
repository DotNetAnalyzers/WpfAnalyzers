namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Attribute
    {
        internal static bool TryGetAttribute(TypeDeclarationSyntax typeDeclaration, QualifiedType attributeType, SemanticModel semanticModel, CancellationToken cancellationToken, out AttributeSyntax result)
        {
            result = null;
            if (typeDeclaration == null)
            {
                return false;
            }

            foreach (var attributeList in typeDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (IsType(attribute, attributeType, semanticModel, cancellationToken))
                    {
                        result = attribute;
                        return true;
                    }
                }
            }

            return false;
        }

        internal static bool IsType(AttributeSyntax attribute, QualifiedType qualifiedType, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            bool IsMatch(SimpleNameSyntax sn, QualifiedType qt)
            {
                return sn.Identifier.ValueText == qt.Type ||
                       StringHelper.IsParts(qt.Type, sn.Identifier.ValueText, "Attribute");
            }

            if (attribute == null)
            {
                return false;
            }

            if (attribute.Name is SimpleNameSyntax simpleName)
            {
                if (!IsMatch(simpleName, qualifiedType) &&
                    !AliasWalker.Contains(attribute.SyntaxTree, simpleName.Identifier.ValueText))
                {
                    return false;
                }
            }
            else if (attribute.Name is QualifiedNameSyntax qualifiedName &&
                     qualifiedName.Right is SimpleNameSyntax typeName)
            {
                if (!IsMatch(typeName, qualifiedType) &&
                    !AliasWalker.Contains(attribute.SyntaxTree, typeName.Identifier.ValueText))
                {
                    return false;
                }
            }

            var attributeType = semanticModel.GetTypeInfoSafe(attribute, cancellationToken).Type;
            return attributeType == qualifiedType;
        }

        internal static bool TryGetArgument(AttributeSyntax attribute, int argumentIndex, string argumentName, out AttributeArgumentSyntax arg)
        {
            arg = null;
            if (attribute?.ArgumentList == null)
            {
                return false;
            }

            if (argumentName != null)
            {
                foreach (var argument in attribute.ArgumentList.Arguments)
                {
                    if (argument.NameColon?.Name.Identifier.ValueText == argumentName)
                    {
                        arg = argument;
                    }
                }
            }

            if (arg != null)
            {
                return true;
            }

            return attribute.ArgumentList.Arguments.TryGetAtIndex(argumentIndex, out arg);
        }

        internal static IEnumerable<AttributeSyntax> FindAttributes(CompilationUnitSyntax assemblyInfo, QualifiedType typeName, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            foreach (var attributeList in assemblyInfo.AttributeLists)
            {
                foreach (var candidate in attributeList.Attributes)
                {
                    if (IsType(candidate, typeName, semanticModel, cancellationToken))
                    {
                        yield return candidate;
                    }
                }
            }
        }
    }
}