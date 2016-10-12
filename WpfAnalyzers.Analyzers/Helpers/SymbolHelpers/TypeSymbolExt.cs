namespace WpfAnalyzers
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class TypeSymbolExt
    {
        internal static bool TryGetField(this ITypeSymbol type, string name, out IFieldSymbol field)
        {
            return type.TryGetSingleMember(name, out field);
        }

        internal static bool TryGetProperty(this ITypeSymbol type, string name, out IPropertySymbol property)
        {
            return type.TryGetSingleMember(name, out property);
        }

        internal static bool TryGetSingleMember<TMember>(this ITypeSymbol type, string name, out TMember member)
            where TMember : class, ISymbol
        {
            member = null;
            if (type == null || string.IsNullOrEmpty(name))
            {
                return false;
            }

            foreach (var symbol in type.GetMembers(name))
            {
                if (member != null)
                {
                    member = null;
                    return false;
                }

                member = symbol as TMember;
            }

            return member != null;
        }

        internal static bool IsSameType(this ITypeSymbol first, ITypeSymbol other)
        {
            if (first == null || other == null)
            {
                return false;
            }

            return first.Equals(other);
        }

        internal static bool IsRepresentationConservingConversion(
            this ITypeSymbol toType,
            ExpressionSyntax valueExpression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            var conversion = semanticModel.SemanticModelFor(valueExpression)
                                          .ClassifyConversion(valueExpression, toType);
            if (!conversion.Exists)
            {
                return false;
            }

            if (conversion.IsIdentity ||
                conversion.IsReference ||
                conversion.IsNullLiteral)
            {
                return true;
            }

            if (toType.IsNullable(valueExpression, semanticModel, cancellationToken))
            {
                return true;
            }

            return false;
        }

        internal static bool IsObject(this ITypeSymbol type)
        {
            return type != null && type.BaseType == null;
        }

        internal static bool IsNullable(this ITypeSymbol nullableType, ExpressionSyntax value, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var namedTypeSymbol = nullableType as INamedTypeSymbol;
            if (namedTypeSymbol == null || !namedTypeSymbol.IsGenericType || namedTypeSymbol.Name != "Nullable" || namedTypeSymbol.TypeParameters.Length != 1)
            {
                return false;
            }

            var typeInfo = semanticModel.SemanticModelFor(value)
                                        .GetTypeInfo(value, cancellationToken);
            return namedTypeSymbol.TypeArguments[0].IsSameType(typeInfo.Type);
        }

        internal static bool IsAssignableToDependencyObject(this ITypeSymbol type)
        {
            while (type?.BaseType != null)
            {
                if (type.Name == Names.DependencyObject)
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }

        internal static bool IsAssignableTo(this ITypeSymbol type, ITypeSymbol other)
        {
            var otherNamedType = other as INamedTypeSymbol;
            while (type?.BaseType != null)
            {
                var namedType = type as INamedTypeSymbol;
                if (namedType?.IsGenericType == true)
                {
                    if (IsSameType(namedType.OriginalDefinition, otherNamedType))
                    {
                        return true;
                    }
                }

                if (IsSameType(type, other))
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}