namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ConstructorArgument
    {
        internal static bool TryGetArgumentName(AttributeSyntax attribute, out AttributeArgumentSyntax argument, out string argumentName)
        {
            argumentName = null;
            if (attribute.TryFindArgument( 0, "argumentName", out argument) &&
                argument.Expression is LiteralExpressionSyntax literal)
            {
                argumentName = literal.Token.ValueText;
            }

            return argumentName != null;
        }

        internal static bool TryGetParameterName(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out string parameterName)
        {
            parameterName = null;
            if (property.TrySingleDeclaration(cancellationToken, out PropertyDeclarationSyntax propertyDeclaration) &&
                propertyDeclaration.TryFirstAncestor<TypeDeclarationSyntax>(out var typeDeclaration))
            {
#pragma warning disable GU0011 // Don't ignore the return value.
                TryGetParameterName(property, typeDeclaration, semanticModel, cancellationToken, out parameterName);
#pragma warning restore GU0011 // Don't ignore the return value.

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
