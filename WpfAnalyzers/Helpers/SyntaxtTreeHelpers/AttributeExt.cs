namespace WpfAnalyzers;

using System.Collections.Generic;
using System.Threading;
using Gu.Roslyn.AnalyzerExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class AttributeExt
{
    internal static IEnumerable<AttributeSyntax> FindAttributes(CompilationUnitSyntax assemblyInfo, QualifiedType expectedType, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        foreach (var attributeList in assemblyInfo.AttributeLists)
        {
            foreach (var candidate in attributeList.Attributes)
            {
                if (semanticModel.TryGetNamedType(candidate, expectedType, cancellationToken, out _))
                {
                    yield return candidate;
                }
            }
        }
    }
}
