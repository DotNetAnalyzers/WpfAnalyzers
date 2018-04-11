namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ValueConverter
    {
        internal static bool TryGetDefaultFieldsOrProperties(ITypeSymbol type, out IReadOnlyList<FieldOrProperty> defaults)
        {
            List<FieldOrProperty> temp = null;
            foreach (var member in type.GetMembers())
            {
                if (member.IsStatic &&
                    (member.DeclaredAccessibility == Accessibility.Public ||
                     member.DeclaredAccessibility == Accessibility.Internal))
                {
                    if (FieldOrProperty.TryCreate(member, out var fieldOrProperty) &&
                        (fieldOrProperty.Type.Is(KnownSymbol.IValueConverter) ||
                         fieldOrProperty.Type.Is(KnownSymbol.IMultiValueConverter)))
                    {
                        if (temp == null)
                        {
                            temp = new List<FieldOrProperty>();
                        }

                        temp.Add(fieldOrProperty);
                    }
                }
            }

            defaults = temp;
            return defaults != null;
        }

        internal static bool TryGetConversionTypes(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol sourceType, out ITypeSymbol targetType)
        {
            sourceType = null;
            targetType = null;
            if (classDeclaration.TryFindMethod("Convert", out var convertMethod) &&
                convertMethod.ReturnType is PredefinedTypeSyntax returnType &&
                returnType.Keyword.ValueText == "object" &&
                convertMethod.ParameterList != null &&
                convertMethod.ParameterList.Parameters.Count == 4)
            {
                using (var walker = ReturnExpressionsWalker.Borrow(convertMethod))
                {
                    using (var returnTypes = PooledSet<ITypeSymbol>.Borrow())
                    {
                        returnTypes.UnionWith(walker.ReturnValues.Select(x => semanticModel.GetTypeInfoSafe(x, cancellationToken).Type));
                        return returnTypes.TrySingle(out targetType) &&
                               ConversionWalker.TryGetCommonBase(
                                   convertMethod,
                                   semanticModel.GetDeclaredSymbolSafe(
                                       convertMethod.ParameterList.Parameters[0],
                                       cancellationToken),
                                   semanticModel,
                                   cancellationToken,
                                   out sourceType);
                    }
                }
            }

            return false;
        }
    }
}
