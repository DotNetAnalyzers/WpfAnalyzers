namespace WpfAnalyzers
{
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ConstructorArgument
    {
        internal static bool? IsMatch(AttributeSyntax attribute, out AttributeArgumentSyntax argument, out string parameterName)
        {
            argument = null;
            parameterName = null;
            var propertyDeclaration = attribute?.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            if (AttributeExt.TryGetArgument(attribute, 0, "argumentName", out argument) &&
                argument.Expression is LiteralExpressionSyntax literal)
            {
                return IsAssigned(propertyDeclaration, out parameterName) &&
                       parameterName == literal.Token.ValueText;
            }

            return true;
        }

        internal static bool IsAssigned(PropertyDeclarationSyntax propertyDeclaration, out string parameterName)
        {
            bool TryGetAssignedName(AssignmentExpressionSyntax assignment, out string name)
            {
                name = null;
                if (assignment.Left is IdentifierNameSyntax identifierName)
                {
                    name = identifierName.Identifier.ValueText;
                }

                if (assignment.Left is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Expression is ThisExpressionSyntax &&
                    memberAccess.Name is IdentifierNameSyntax nameSyntax)
                {
                    name = nameSyntax.Identifier.ValueText;
                }

                return name != null;
            }

            parameterName = null;
            var typeDeclaration = propertyDeclaration?.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            if (typeDeclaration == null)
            {
                return false;
            }

            var backingFieldName = "<missing>";
            if (propertyDeclaration.TryGetSetter(out var setter) &&
                AssignmentWalker.TrySingle(setter, out var fieldAssignment) &&
                TryGetAssignedName(fieldAssignment, out backingFieldName))
            {
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

                    if (TryGetAssignedName(assignment, out var name) &&
                        (propertyDeclaration.Identifier.ValueText == name ||
                         backingFieldName == name))
                    {
                        if (assignment.Right is IdentifierNameSyntax candidate &&
                            ctor.ParameterList.Parameters.TrySingle(x => x.Identifier.ValueText == candidate.Identifier.ValueText, out var parameter))
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
            }

            return parameterName != null;
        }
    }
}
