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
                var type = semanticModel.GetTypeInfoSafe(returnValue, cancellationToken).Type;
                if (type == KnownSymbol.Object &&
                    semanticModel.GetSymbolSafe(returnValue, cancellationToken) is ISymbol symbol &&
                    symbol.IsEither<IFieldSymbol, IPropertySymbol>())
                {
                    switch (symbol)
                    {
                    case IFieldSymbol field:
                        if (field.Type == KnownSymbol.Object &&
                            field.DeclaredAccessibility == Accessibility.Private &&
                            returnValue.FirstAncestor<TypeDeclarationSyntax>() is TypeDeclarationSyntax typeDeclaration)
                        {
                            using (var walker = AssignmentExecutionWalker.Borrow(typeDeclaration, Search.TopLevel, semanticModel, cancellationToken))
                            {
                                foreach (var assignment in walker.Assignments)
                                {
                                    if (SymbolComparer.Equals(semanticModel.GetSymbolSafe(assignment.Left, cancellationToken), field))
                                    {
                                        returnTypes.Add(semanticModel.GetTypeInfoSafe(assignment.Right, cancellationToken).Type);
                                    }
                                }
                            }
                        }
                        else
                        {
                            returnTypes.Add(field.Type);
                        }

                        return;
                    case IPropertySymbol property:
                        returnTypes.Add(property.Type);
                        return;
                    }
                }

                returnTypes.Add(type);
            }
        }
    }
}
