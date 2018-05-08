namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ConstructorArgument
    {
        internal static bool IsMatch(AttributeSyntax attribute, SemanticModel semanticModel, CancellationToken cancellationToken, out AttributeArgumentSyntax argument, out string parameterName)
        {
            argument = null;
            parameterName = null;
            if (Attribute.TryFindArgument(attribute, 0, "argumentName", out argument) &&
                argument.Expression is LiteralExpressionSyntax literal &&
                attribute.TryFirstAncestor<PropertyDeclarationSyntax>(out var propertyDeclaration) &&
                semanticModel.TryGetSymbol(propertyDeclaration, cancellationToken, out var property))
            {
                return TryGetParameterName(property, semanticModel, cancellationToken, out parameterName) &&
                       parameterName == literal.Token.ValueText;
            }

            return true;
        }

        internal static bool TryGetParameterName(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out string parameterName)
        {
            parameterName = null;
            if (property.TrySingleDeclaration(cancellationToken, out PropertyDeclarationSyntax propertyDeclaration) &&
                propertyDeclaration.TryFirstAncestor<TypeDeclarationSyntax>(out var typeDeclaration))
            {
                TryGetParameterName(property, typeDeclaration, semanticModel, cancellationToken, out parameterName);

                if (propertyDeclaration.TryGetBackingField(out var backingField) &&
                    semanticModel.TryGetSymbol(backingField, cancellationToken, out var field))
                {
                    if (TryGetParameterName(field, typeDeclaration, semanticModel, cancellationToken, out var candidate))
                    {
                        if (parameterName == null)
                        {
                            parameterName = candidate;
                        }
                        else if (parameterName != candidate)
                        {
                            return false;
                        }
                    }
                }
            }

            return parameterName != null;
        }

        private static bool TryGetParameterName(ISymbol member, TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out string parameterName)
        {
            parameterName = null;
            using (var walker = AssignmentExecutionWalker.For(member, typeDeclaration, Scope.Member, semanticModel, cancellationToken))
            {
                foreach (var assignment in walker.Assignments)
                {
                    if (assignment.Right is IdentifierNameSyntax identifierName &&
                        assignment.TryFirstAncestor<ConstructorDeclarationSyntax>(out var ctor) &&
                        ctor.TryFindParameter(identifierName.Identifier.ValueText, out _))
                    {
                        if (parameterName == null)
                        {
                            parameterName = identifierName.Identifier.ValueText;
                        }
                        else if (parameterName != identifierName.Identifier.ValueText)
                        {
                            parameterName = null;
                            return false;
                        }
                    }
                }
            }

            return parameterName != null;
        }
    }
}
