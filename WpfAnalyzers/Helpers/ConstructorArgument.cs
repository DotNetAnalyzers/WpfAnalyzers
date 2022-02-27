namespace WpfAnalyzers;

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Gu.Roslyn.AnalyzerExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class ConstructorArgument
{
    internal static bool TryGetArgumentName(AttributeSyntax attribute, [NotNullWhen(true)] out AttributeArgumentSyntax? argument, [NotNullWhen(true)] out string? argumentName)
    {
        argumentName = null;
        if (attribute.TryFindArgument(0, "argumentName", out argument) &&
            argument.Expression is LiteralExpressionSyntax literal)
        {
            argumentName = literal.Token.ValueText;
        }

        return argumentName is { };
    }

    internal static bool TryGetParameterName(IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out string? parameterName)
    {
        if (property.TrySingleDeclaration(cancellationToken, out PropertyDeclarationSyntax? propertyDeclaration) &&
            propertyDeclaration.TryFirstAncestor<TypeDeclarationSyntax>(out var typeDeclaration))
        {
            if (TryGetParameterName(property, typeDeclaration, semanticModel, cancellationToken, out parameterName))
            {
                return true;
            }

            return propertyDeclaration.TryGetBackingField(out var backingField) &&
                   semanticModel.TryGetSymbol(backingField, cancellationToken, out var field) &&
                   TryGetParameterName(field, typeDeclaration, semanticModel, cancellationToken, out parameterName);
        }

        parameterName = null;
        return false;
    }

    private static bool TryGetParameterName(ISymbol member, TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out string? parameterName)
    {
        using (var walker = AssignmentExecutionWalker.For(member, typeDeclaration, SearchScope.Member, semanticModel, cancellationToken))
        {
            foreach (var assignment in walker.Assignments)
            {
                if (assignment.Right is IdentifierNameSyntax { Identifier: { } identifier } &&
                    assignment.TryFirstAncestor<ConstructorDeclarationSyntax>(out var ctor) &&
                    ctor.TryFindParameter(identifier.ValueText, out var parameter))
                {
                    parameterName = parameter.Identifier.ValueText;
                    return true;
                }
            }
        }

        parameterName = null;
        return false;
    }
}
