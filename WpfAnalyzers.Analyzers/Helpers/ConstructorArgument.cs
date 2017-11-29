namespace WpfAnalyzers
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ConstructorArgument
    {
        internal static bool TryGetAttribute(PropertyDeclarationSyntax propertyDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out AttributeSyntax result)
        {
            result = null;
            if (propertyDeclaration == null)
            {
                return false;
            }

            foreach (var attributeList in propertyDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (Attribute.IsType(attribute, KnownSymbol.ConstructorArgumentAttribute, semanticModel, cancellationToken))
                    {
                        result = attribute;
                        return true;
                    }
                }
            }

            return false;
        }

        internal static bool? IsMatch(AttributeSyntax attribute, out AttributeArgumentSyntax argument, out string parameterName)
        {
            argument = null;
            parameterName = null;
            var propertyDeclaration = attribute?.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            if (Attribute.TryGetArgument(attribute, 0, "argumentName", out argument) &&
                argument.Expression is LiteralExpressionSyntax literal)
            {
                return IsAssigned(propertyDeclaration, out parameterName) &&
                       parameterName == literal.Token.ValueText;
            }

            return true;
        }

        internal static bool IsAssigned(PropertyDeclarationSyntax propertyDeclaration, out string parameterName)
        {
            parameterName = null;
            var typeDeclaration = propertyDeclaration?.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            if (typeDeclaration == null)
            {
                return false;
            }

            using (var walker = AssignmentWalker.Borrow(typeDeclaration))
            {
                foreach (var assignment in walker.Assignments)
                {
                    var ctor = assignment.FirstAncestor<ConstructorDeclarationSyntax>();
                    if (ctor?.ParameterList == null)
                    {
                        continue;
                    }

                    if (assignment.Left is IdentifierNameSyntax identifierName &&
                        propertyDeclaration.Identifier.ValueText != identifierName.Identifier.ValueText)
                    {
                        continue;
                    }

                    if (assignment.Left is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Expression is ThisExpressionSyntax &&
                        memberAccess.Name is IdentifierNameSyntax nameSyntax &&
                        propertyDeclaration.Identifier.ValueText != nameSyntax.Identifier.ValueText)
                    {
                        continue;
                    }

                    if (assignment.Right is IdentifierNameSyntax candidate &&
                        ctor.ParameterList.Parameters.TryGetSingle(x => x.Identifier.ValueText == candidate.Identifier.ValueText, out var parameter))
                    {
                        if (parameterName != null &&
                            parameterName != parameter.Identifier.ValueText)
                        {
                            parameterName += ", " + parameter.Identifier.ValueText;
                        }
                        else
                        {
                            parameterName = parameter.Identifier.ValueText;
                        }
                    }
                }
            }

            return parameterName != null;
        }
    }
}