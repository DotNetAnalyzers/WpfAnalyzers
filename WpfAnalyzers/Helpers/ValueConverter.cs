namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class ValueConverter
    {
        internal static bool TryGetDefaultFieldsOrProperties(ITypeSymbol type, Compilation compilation, out IReadOnlyList<FieldOrProperty> defaults)
        {
            List<FieldOrProperty> temp = null;
            foreach (var member in type.GetMembers())
            {
                if (member.IsStatic &&
                    (member.DeclaredAccessibility == Accessibility.Public ||
                     member.DeclaredAccessibility == Accessibility.Internal))
                {
                    if (FieldOrProperty.TryCreate(member, out var fieldOrProperty) &&
                        (fieldOrProperty.Type.IsAssignableTo(KnownSymbols.IValueConverter, compilation) ||
                         fieldOrProperty.Type.IsAssignableTo(KnownSymbols.IMultiValueConverter, compilation)))
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
                switch (returnValue)
                {
                    case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.NullLiteralExpression):
                        break;
                    case ConditionalExpressionSyntax ternary:
                        AddReturnType(returnTypes, ternary.WhenTrue);
                        AddReturnType(returnTypes, ternary.WhenFalse);
                        break;
                    case BinaryExpressionSyntax coalesce when coalesce.IsKind(SyntaxKind.CoalesceExpression):
                        AddReturnType(returnTypes, coalesce.Left);
                        AddReturnType(returnTypes, coalesce.Right);
                        break;
                    case IdentifierNameSyntax _:
                    case MemberAccessExpressionSyntax _:
                        var type = semanticModel.GetTypeInfoSafe(returnValue, cancellationToken).Type;
                        if (type == KnownSymbols.Object &&
                            semanticModel.GetSymbolSafe(returnValue, cancellationToken) is ISymbol symbol &&
                            symbol.IsEither<IFieldSymbol, IPropertySymbol>())
                        {
                            switch (symbol)
                            {
                                case IFieldSymbol field:
                                    if (field.Type == KnownSymbols.Object &&
                                        field.DeclaredAccessibility == Accessibility.Private &&
                                        returnValue.FirstAncestor<TypeDeclarationSyntax>() is TypeDeclarationSyntax typeDeclaration)
                                    {
                                        using (var walker = AssignmentExecutionWalker.Borrow(typeDeclaration, SearchScope.Instance, semanticModel, cancellationToken))
                                        {
                                            foreach (var assignment in walker.Assignments)
                                            {
                                                if (semanticModel.TryGetSymbol(assignment.Left, cancellationToken, out IFieldSymbol assigned) &&
                                                    FieldSymbolComparer.Equals(assigned, field))
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
                        else
                        {
                            returnTypes.Add(type);
                        }

                        break;
                    default:
                        returnTypes.Add(semanticModel.GetTypeInfoSafe(returnValue, cancellationToken).Type);
                        break;
                }
            }
        }
    }
}
