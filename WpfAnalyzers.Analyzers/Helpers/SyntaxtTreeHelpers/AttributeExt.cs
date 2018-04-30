namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class AttributeExt
    {
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

            return attribute.ArgumentList.Arguments.TryElementAt(argumentIndex, out arg);
        }

        internal static bool TrySingleArgument(this AttributeSyntax attribute, out AttributeArgumentSyntax argument)
        {
            var argumentList = attribute?.ArgumentList;
            if (argumentList == null)
            {
                argument = null;
                return false;
            }

            return argumentList.Arguments.TrySingle(out argument);
        }

        internal static IEnumerable<AttributeSyntax> FindAttributes(CompilationUnitSyntax assemblyInfo, QualifiedType typeName, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            foreach (var attributeList in assemblyInfo.AttributeLists)
            {
                foreach (var candidate in attributeList.Attributes)
                {
                    if (Attribute.IsType(candidate, typeName, semanticModel, cancellationToken))
                    {
                        yield return candidate;
                    }
                }
            }
        }
    }
}
