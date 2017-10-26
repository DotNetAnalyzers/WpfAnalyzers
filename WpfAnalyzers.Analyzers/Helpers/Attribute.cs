namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Attribute
    {
        internal static bool TryGetAttribute(AttributeSyntax attribute, QualifiedType attributeName, SemanticModel semanticModel, CancellationToken cancellationToken, out AttributeSyntax result)
        {
            result = null;
            var attributeType = semanticModel.GetTypeInfoSafe(attribute, cancellationToken).Type;
            if (attributeType != attributeName)
            {
                return false;
            }

            result = attribute;
            return true;
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
                    if (TryGetAttribute(candidate, typeName, semanticModel, cancellationToken, out var attribute))
                    {
                        yield return attribute;
                    }
                }
            }
        }
    }
}