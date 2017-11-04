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
                        fieldOrProperty.Type.Is(KnownSymbol.IValueConverter))
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

        internal static bool TryGetConversionTypes(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol inType, out ITypeSymbol outType)
        {
            inType = null;
            outType = null;
            if (classDeclaration.TryFindMethod("Convert", out var convertMethod) &&
                convertMethod.ReturnType is PredefinedTypeSyntax returnType &&
                returnType.Keyword.ValueText == "object" &&
                convertMethod.ParameterList != null &&
                convertMethod.ParameterList.Parameters.Count == 4)
            {
                using (var walker = ReturnExpressionsWalker.Borrow(convertMethod))
                {
                    using (var returnTypes = PooledHashSet<ITypeSymbol>.Borrow())
                    {
                        returnTypes.UnionWith(walker.ReturnValues.Select(x => semanticModel.GetTypeInfoSafe(x, cancellationToken).Type));
                        if (returnTypes.TryGetSingle(out outType) &&
                            ConversionWalker.TryGetSingle(
                                convertMethod,
                                semanticModel.GetDeclaredSymbolSafe(convertMethod.ParameterList.Parameters[0], cancellationToken),
                                out var inTypeSyntax))
                        {
                            inType = semanticModel.GetTypeInfoSafe(inTypeSyntax, cancellationToken)
                                                  .Type;
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
