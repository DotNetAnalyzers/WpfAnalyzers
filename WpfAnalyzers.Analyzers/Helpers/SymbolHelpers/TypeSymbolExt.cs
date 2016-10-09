namespace WpfAnalyzers
{
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class TypeSymbolExt
    {
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
            var conversion = semanticModel.ClassifyConversion(valueExpression, toType);
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

        internal static bool IsNullable(this ITypeSymbol nullableType, ExpressionSyntax value, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var namedTypeSymbol = nullableType as INamedTypeSymbol;
            if (namedTypeSymbol == null || !namedTypeSymbol.IsGenericType || namedTypeSymbol.Name != "Nullable" || namedTypeSymbol.TypeParameters.Length != 1)
            {
                return false;
            }

            var typeInfo = semanticModel.GetTypeInfo(value, cancellationToken);
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
            while (type?.BaseType != null)
            {
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