namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal static class TypeSymbolExt
    {
        internal static IEnumerable<ITypeSymbol> RecursiveBaseTypes(this ITypeSymbol type)
        {
            while (type != null)
            {
                foreach (var @interface in type.AllInterfaces)
                {
                    yield return @interface;
                }

                type = type.BaseType;
                if (type != null)
                {
                    yield return type;
                }
            }
        }

        internal static IEnumerable<ISymbol> RecursiveMembers(this ITypeSymbol type)
        {
            while (type != null)
            {
                foreach (var member in type.GetMembers())
                {
                    yield return member;
                }

                type = type.BaseType;
            }
        }

        internal static IEnumerable<ISymbol> RecursiveMembers(this ITypeSymbol type, string name)
        {
            while (type != null)
            {
                foreach (var member in type.GetMembers(name))
                {
                    yield return member;
                }

                type = type.BaseType;
            }
        }

        internal static bool TryGetFieldRecursive(this ITypeSymbol type, string name, out IFieldSymbol field)
        {
            return type.TryGetSingleMemberRecursive(name, out field);
        }

        internal static bool TryGetPropertyRecursive(this ITypeSymbol type, string name, out IPropertySymbol property)
        {
            return type.TryGetSingleMemberRecursive(name, out property);
        }

        internal static bool TryGetSingleMemberRecursive<TMember>(this ITypeSymbol type, string name, out TMember member)
            where TMember : class, ISymbol
        {
            member = null;
            if (type == null ||
                string.IsNullOrEmpty(name))
            {
                return false;
            }

            foreach (var symbol in type.RecursiveMembers(name))
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
            if (ReferenceEquals(first, other) ||
                first?.Equals(other) == true)
            {
                return true;
            }

            if (first is ITypeParameterSymbol firstParameter &&
                other is ITypeParameterSymbol otherParameter)
            {
                return firstParameter.MetadataName == otherParameter.MetadataName &&
                       firstParameter.ContainingSymbol.Equals(otherParameter.ContainingSymbol);
            }

            return first is INamedTypeSymbol firstNamed &&
                   other is INamedTypeSymbol otherNamed &&
                   IsSameType(firstNamed, otherNamed);
        }

        internal static bool IsSameType(this INamedTypeSymbol first, INamedTypeSymbol other)
        {
            if (first == null ||
                other == null)
            {
                return false;
            }

            if (first.IsDefinition ^ other.IsDefinition)
            {
                return IsSameType(first.OriginalDefinition, other.OriginalDefinition);
            }

            return first.Equals(other) ||
                   AreEquivalent(first, other);
        }

        internal static bool IsRepresentationPreservingConversion(
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

            if (conversion.IsIdentity)
            {
                return true;
            }

            if (conversion.IsReference &&
                conversion.IsImplicit)
            {
                return true;
            }

            if (conversion.IsNullable &&
                conversion.IsNullLiteral)
            {
                return true;
            }

            if (conversion.IsBoxing ||
                conversion.IsUnboxing)
            {
                if (valueExpression is CastExpressionSyntax castExpression)
                {
                    return IsRepresentationPreservingConversion(
                        toType,
                        castExpression.Expression,
                        semanticModel,
                        cancellationToken);
                }

                return true;
            }

            if (toType.IsNullable(valueExpression, semanticModel, cancellationToken))
            {
                return true;
            }

            return false;
        }

        internal static bool IsNullable(
            this ITypeSymbol nullableType,
            ExpressionSyntax value,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if (!(nullableType is INamedTypeSymbol namedTypeSymbol) ||
                !namedTypeSymbol.IsGenericType ||
                namedTypeSymbol.Name != "Nullable" ||
                namedTypeSymbol.TypeParameters.Length != 1)
            {
                return false;
            }

            if (value.IsKind(SyntaxKind.NullLiteralExpression))
            {
                return true;
            }

            var typeInfo = semanticModel.GetTypeInfoSafe(value, cancellationToken);
            return namedTypeSymbol.TypeArguments[0].IsSameType(typeInfo.Type);
        }

        internal static bool IsEither(this ITypeSymbol type, QualifiedType qualifiedType1, QualifiedType qualifiedType2)
        {
            return type.Is(qualifiedType1) || type.Is(qualifiedType2);
        }

        internal static bool Is(this ITypeSymbol type, QualifiedType qualifiedType)
        {
            while (type != null)
            {
                if (type == qualifiedType)
                {
                    return true;
                }

                foreach (var @interface in type.AllInterfaces)
                {
                    if (@interface == qualifiedType)
                    {
                        return true;
                    }
                }

                type = type.BaseType;
            }

            return false;
        }

        internal static bool Is(this ITypeSymbol type, ITypeSymbol other)
        {
            if (type == null ||
                other == null)
            {
                return false;
            }

            if (IsSameType(type, other))
            {
                return true;
            }

            if (type == KnownSymbol.Nullable)
            {
                return type is INamedTypeSymbol namedType &&
                       Equals(namedType.TypeArguments[0], other);
            }

            if (other.IsInterface())
            {
                foreach (var @interface in type.AllInterfaces)
                {
                    if (IsSameType(@interface, other))
                    {
                        return true;
                    }
                }

                return false;
            }

            while (type?.BaseType != null)
            {
                if (IsSameType(type, other))
                {
                    return true;
                }

                type = type.BaseType;
            }

            if (other == KnownSymbol.Object)
            {
                return true;
            }

            return false;
        }

        internal static bool IsInterface(this ITypeSymbol type)
        {
            if (type == null)
            {
                return false;
            }

            return type != KnownSymbol.Object && type.BaseType == null;
        }

        internal static bool AreEquivalent(this INamedTypeSymbol first, INamedTypeSymbol other)
        {
            if (ReferenceEquals(first, other))
            {
                return true;
            }

            if (first == null ||
                other == null)
            {
                return false;
            }

            if (first.MetadataName != other.MetadataName ||
                first.ContainingModule.MetadataName != other.ContainingModule.MetadataName ||
                first.Arity != other.Arity)
            {
                return false;
            }

            for (var i = 0; i < first.Arity; i++)
            {
                if (!IsSameType(first.TypeArguments[i], other.TypeArguments[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}