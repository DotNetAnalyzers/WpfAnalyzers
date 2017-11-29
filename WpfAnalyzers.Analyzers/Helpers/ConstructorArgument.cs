namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ConstructorArgument
    {
        internal static bool? IsMatch(AttributeSyntax attribute, out AttributeArgumentSyntax argument, out string parameterName)
        {
            argument = null;
            parameterName = null;
            var propertyDeclaration = attribute?.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            var typeDeclaration = propertyDeclaration?.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            if (typeDeclaration == null)
            {
                return null;
            }

            if (Attribute.TryGetArgument(attribute, 0, "argumentName", out argument) &&
                argument.Expression is LiteralExpressionSyntax literal)
            {
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
                            if (parameter.Identifier.ValueText != literal.Token.ValueText)
                            {
                                parameterName = parameter.Identifier.ValueText;
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}