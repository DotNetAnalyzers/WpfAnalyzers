﻿namespace WpfAnalyzers;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using Gu.Roslyn.AnalyzerExtensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class ValueConverter
{
    internal static bool TryGetDefaultFieldsOrProperties(ITypeSymbol type, Compilation compilation, [NotNullWhen(true)] out IReadOnlyList<FieldOrProperty>? defaults)
    {
        List<FieldOrProperty>? temp = null;
        foreach (var member in type.GetMembers())
        {
            if (member.IsStatic &&
                member.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal)
            {
                if (FieldOrProperty.TryCreate(member, out var fieldOrProperty) &&
                    (fieldOrProperty.Type.IsAssignableTo(KnownSymbols.IValueConverter,      compilation) ||
                     fieldOrProperty.Type.IsAssignableTo(KnownSymbols.IMultiValueConverter, compilation)))
                {
                    temp ??= new List<FieldOrProperty>();
                    temp.Add(fieldOrProperty);
                }
            }
        }

        defaults = temp;
        return defaults is { };
    }

    internal static bool TryGetConversionTypes(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out ITypeSymbol? sourceType, [NotNullWhen(true)] out ITypeSymbol? targetType)
    {
        sourceType = null;
        targetType = null;
        if (classDeclaration.TryFindMethod("Convert", out var convertMethod) &&
            convertMethod is { ReturnType: { } returnType, ParameterList.Parameters: { Count: 4 } parameters } &&
            returnType == KnownSymbols.Object &&
            parameters.TryFirst<ParameterSyntax>(out var valueParameter))
        {
            using var returnValues = ReturnValueWalker.Borrow(convertMethod);
            using var returnTypes = PooledSet<ITypeSymbol>.Borrow();
            foreach (var returnValue in returnValues.ReturnValues)
            {
                AddReturnType(returnTypes, returnValue);
            }

            return returnTypes.TrySingle(out targetType) &&
                   semanticModel.TryGetSymbol(valueParameter, cancellationToken, out var symbol) &&
                   ConversionWalker.TryGetCommonBase(convertMethod, symbol, semanticModel, cancellationToken, out sourceType);
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
                    var type = semanticModel.GetType(returnValue, cancellationToken);
                    if (type == KnownSymbols.Object &&
                        semanticModel.GetSymbolSafe(returnValue, cancellationToken) is { } symbol &&
                        symbol.IsEither<IFieldSymbol, IPropertySymbol>())
                    {
                        switch (symbol)
                        {
                            case IFieldSymbol field:
                                if (field.Type == KnownSymbols.Object &&
                                    field.DeclaredAccessibility == Accessibility.Private &&
                                    returnValue.TryFirstAncestor(out TypeDeclarationSyntax? typeDeclaration))
                                {
                                    using var walker = AssignmentExecutionWalker.Borrow(typeDeclaration, SearchScope.Instance, semanticModel, cancellationToken);
                                    foreach (var assignment in walker.Assignments)
                                    {
                                        if (semanticModel.TryGetSymbol(assignment.Left, cancellationToken, out IFieldSymbol? assigned) &&
                                            FieldSymbolComparer.Equal(assigned, field) &&
                                            semanticModel.GetType(assignment.Right, cancellationToken) is { } rightType)
                                        {
                                            _ = returnTypes.Add(rightType);
                                        }
                                    }
                                }
                                else
                                {
                                    _ = returnTypes.Add(field.Type);
                                }

                                return;
                            case IPropertySymbol property:
                                _ = returnTypes.Add(property.Type);
                                return;
                        }
                    }
                    else if (type is { })
                    {
                        _ = returnTypes.Add(type);
                    }

                    break;
                case ThrowExpressionSyntax _:
                    break;
                case SwitchExpressionSyntax switchExpression:
                    foreach (var arm in switchExpression.Arms)
                    {
                        AddReturnType(returnTypes, arm.Expression);
                    }

                    break;
                default:
                    if (semanticModel.GetType(returnValue, cancellationToken) is { } returnValueType)
                    {
                        _ = returnTypes.Add(returnValueType);
                    }

                    break;
            }
        }
    }
}
