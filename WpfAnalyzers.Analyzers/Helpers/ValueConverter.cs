namespace WpfAnalyzers
{
    using System.Collections.Generic;
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
                convertMethod.ParameterList.Parameters.Count == 4 &&
                convertMethod.ParameterList.Parameters.TryFirst(out var valueParameter))
            {
                using (var returnValues = ReturnValueWalker.Borrow(convertMethod))
                {
                    using (var returnTypes = PooledSet<ITypeSymbol>.Borrow())
                    {
                        foreach (var returnValue in returnValues.ReturnValues)
                        {
                            AddReturnType(returnTypes, returnValue);
                        }

                        return returnTypes.TrySingle(out targetType) &&
                               ConversionWalker.TryGetCommonBase(
                                   convertMethod,
                                   semanticModel.GetDeclaredSymbolSafe(valueParameter, cancellationToken),
                                   semanticModel,
                                   cancellationToken,
                                   out sourceType);
                    }
                }
            }

            return false;

            void AddReturnType(PooledSet<ITypeSymbol> returnTypes, ExpressionSyntax returnValue)
            {
                returnTypes.Add(semanticModel.GetTypeInfoSafe(returnValue, cancellationToken).Type);
                //if (returnValue is IdentifierNameSyntax identifierName)
                //{
                //    returnTypes.Add(semanticModel.GetTypeInfoSafe(returnValue, cancellationToken).Type);
                //}
                //else if (returnValue is MemberAccessExpressionSyntax memberAccess)
                //{
                //    returnTypes.Add(semanticModel.GetTypeInfoSafe(returnValue, cancellationToken).Type);
                //}
                //else
                //{
                //    returnTypes.Add(semanticModel.GetTypeInfoSafe(returnValue, cancellationToken).Type);
                //}
            }
        }
    }
}
