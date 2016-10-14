namespace WpfAnalyzers.DependencyProperties
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class Attribute
    {
        internal static bool TryGetAttribute(AttributeSyntax attribute, string attributeName, SemanticModel semanticModel, CancellationToken cancellationToken, out AttributeSyntax result)
        {
            result = null;
            var expectedType = semanticModel.Compilation.GetTypeByMetadataName(attributeName);
            if (expectedType == null)
            {
                return false;
            }

            var attributeType = ModelExtensions.GetTypeInfo(semanticModel.SemanticModelFor(attribute), attribute, cancellationToken)
                               .Type;
            if (attributeType == null ||
                !attributeType.Equals(expectedType))
            {
                return false;
            }

            result = attribute;
            return true;
        }

        public static bool TryGetArgumentStringValue(AttributeSyntax attribute, int argumentIndex, SemanticModel semanticModel, CancellationToken cancellationToken, out string result)
        {
            result = null;
            AttributeArgumentSyntax arg = null;
            if (attribute?.ArgumentList?.Arguments.TryGetAtIndex(argumentIndex, out arg) != true)
            {
                return false;
            }

            if (!arg.Expression.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return false;
            }

            var constantValue = semanticModel.GetConstantValue(arg.Expression, cancellationToken);
            if (!constantValue.HasValue)
            {
                return false;
            }

            result = (string)constantValue.Value;
            return true;
        }
    }
}