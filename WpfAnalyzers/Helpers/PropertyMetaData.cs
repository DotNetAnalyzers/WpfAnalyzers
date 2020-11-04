namespace WpfAnalyzers
{
    using System.Threading;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal readonly struct PropertyMetadata
    {
        internal readonly ObjectCreationExpressionSyntax ObjectCreation;
        internal readonly IMethodSymbol Constructor;

        internal PropertyMetadata(ObjectCreationExpressionSyntax objectCreation, IMethodSymbol constructor)
        {
            this.ObjectCreation = objectCreation;
            this.Constructor = constructor;
        }

        internal ArgumentSyntax? DefaultValueArgument
        {
            get
            {
                if (this.ObjectCreation.ArgumentList is null ||
                    this.ObjectCreation.ArgumentList.Arguments.Count == 0)
                {
                    return null;
                }

                if (this.Constructor.TryFindParameter(KnownSymbols.Object, out var parameter) &&
                    this.ObjectCreation.TryFindArgument(parameter, out var argument))
                {
                    return argument;
                }

                return null;
            }
        }

        internal ArgumentSyntax? PropertyChangedArgument
        {
            get
            {
                if (this.ObjectCreation.ArgumentList is null ||
                    this.ObjectCreation.ArgumentList.Arguments.Count == 0)
                {
                    return null;
                }

                if (this.Constructor.TryFindParameter(KnownSymbols.PropertyChangedCallback, out var parameter) &&
                    this.ObjectCreation.TryFindArgument(parameter, out var argument))
                {
                    return argument;
                }

                return null;
            }
        }

        internal ArgumentSyntax? CoerceValueArgument
        {
            get
            {
                if (this.ObjectCreation.ArgumentList is null ||
                    this.ObjectCreation.ArgumentList.Arguments.Count == 0)
                {
                    return null;
                }

                if (this.Constructor.TryFindParameter(KnownSymbols.CoerceValueCallback, out var parameter) &&
                    this.ObjectCreation.TryFindArgument(parameter, out var argument))
                {
                    return argument;
                }

                return null;
            }
        }

        internal static PropertyMetadata? Match(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (objectCreation is { ArgumentList: { } })
            {
                if (semanticModel.TryGetSymbol(objectCreation, KnownSymbols.PropertyMetadata, cancellationToken, out var constructor) ||
                    semanticModel.TryGetSymbol(objectCreation, KnownSymbols.UIPropertyMetadata, cancellationToken, out constructor) ||
                    semanticModel.TryGetSymbol(objectCreation, KnownSymbols.FrameworkPropertyMetadata, cancellationToken, out constructor))
                {
                    return new PropertyMetadata(objectCreation, constructor);
                }
            }

            return null;
        }

        internal static ArgumentAndValue<string?>? FindRegisteredName(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (Match(objectCreation, semanticModel, cancellationToken) is { } &&
                objectCreation.TryFirstAncestor(out InvocationExpressionSyntax? invocation))
            {
                return DependencyProperty.TryGetRegisteredName(invocation, semanticModel, cancellationToken);
            }

            return null;
        }

        internal static bool TryGetDependencyProperty(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty fieldOrProperty)
        {
            fieldOrProperty = default;
            var invocation = objectCreation.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocation is null)
            {
                return false;
            }

            if (DependencyProperty.Register.MatchAny(invocation, semanticModel, cancellationToken) is { })
            {
                if (objectCreation.TryFirstAncestor<FieldDeclarationSyntax>(out var fieldDeclaration) &&
                    semanticModel.TryGetSymbol(fieldDeclaration, cancellationToken, out var field))
                {
                    return BackingFieldOrProperty.TryCreateForDependencyProperty(field, out fieldOrProperty);
                }

                if (objectCreation.TryFirstAncestor<PropertyDeclarationSyntax>(out var propertyDeclaration) &&
                    semanticModel.TryGetSymbol(propertyDeclaration, cancellationToken, out var property))
                {
                    return BackingFieldOrProperty.TryCreateForDependencyProperty(property, out fieldOrProperty);
                }

                return false;
            }

            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                (DependencyProperty.AddOwner.Match(invocation, semanticModel, cancellationToken) is { } ||
                 DependencyProperty.TryGetOverrideMetadataCall(invocation, semanticModel, cancellationToken, out _)) &&
                semanticModel.TryGetSymbol(memberAccess.Expression, cancellationToken, out var candidate))
            {
                return BackingFieldOrProperty.TryCreateForDependencyProperty(candidate, out fieldOrProperty);
            }

            return false;
        }

        internal static bool IsValueValidForRegisteredType(ExpressionSyntax value, ITypeSymbol registeredType, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (value.FirstAncestor<TypeDeclarationSyntax>() is { } containingTypeDeclaration &&
                semanticModel.TryGetNamedType(containingTypeDeclaration, cancellationToken, out var containingType))
            {
                using var recursion = Recursion.Borrow(containingType, semanticModel, cancellationToken);
                return IsValueValidForRegisteredType(value, registeredType, recursion);
            }

            return true;

            static bool IsValueValidForRegisteredType(ExpressionSyntax value, ITypeSymbol registeredType, Recursion recursion)
            {
                switch (value)
                {
                    case ConditionalExpressionSyntax { WhenTrue: { } whenTrue, WhenFalse: { } whenFalse }:
                        return IsValueValidForRegisteredType(whenTrue, registeredType, recursion) &&
                               IsValueValidForRegisteredType(whenFalse, registeredType, recursion);

                    case BinaryExpressionSyntax { Left: { }, Right: { } right } binary
                        when binary.IsKind(SyntaxKind.CoalesceExpression):
                        return IsValueValidForRegisteredType(right, registeredType, recursion);
                }

                if (registeredType.TypeKind == TypeKind.Enum)
                {
                    return recursion.SemanticModel.TryGetType(value, recursion.CancellationToken, out var valueType) &&
                           valueType.MetadataName == registeredType.MetadataName &&
                           TypeSymbolComparer.Equal(valueType.ContainingType, registeredType.ContainingType) &&
                           NamespaceSymbolComparer.Equal(valueType.ContainingNamespace, registeredType.ContainingNamespace);
                }

                if (recursion.SemanticModel.IsRepresentationPreservingConversion(value, registeredType))
                {
                    return true;
                }

                if (recursion.Target<ExpressionSyntax, ISymbol, CSharpSyntaxNode>(value) is { } target)
                {
                    switch (target)
                    {
                        case { Symbol: IFieldSymbol field, Declaration: VariableDeclaratorSyntax declarator }:
                            if (declarator.Initializer is { Value: { } fv } &&
                                !IsValueValidForRegisteredType(fv, registeredType, recursion))
                            {
                                return false;
                            }

                            return IsAssignedValueOfRegisteredType(field, declarator);

                        case { Symbol: IPropertySymbol property, Declaration: PropertyDeclarationSyntax declaration }:
                            if (declaration.Initializer is { Value: { } pv } &&
                                !IsValueValidForRegisteredType(pv, registeredType, recursion))
                            {
                                return false;
                            }

                            if (declaration.Getter() is { } getter)
                            {
                                return IsReturnValueOfRegisteredType(getter);
                            }

                            return IsAssignedValueOfRegisteredType(property, declaration);
                        case { Symbol: IMethodSymbol _, Declaration: MethodDeclarationSyntax declaration }:
                            return IsReturnValueOfRegisteredType(declaration);
                        case { Symbol: IFieldSymbol { Type: { SpecialType: SpecialType.System_Object } } }:
                            return true;
                        case { Symbol: IPropertySymbol { Type: { SpecialType: SpecialType.System_Object } } }:
                            return true;
                        case { Symbol: IMethodSymbol { ReturnType: { SpecialType: SpecialType.System_Object } } }:
                            return true;
                        default:
                            return recursion.SemanticModel.IsRepresentationPreservingConversion(value, registeredType);
                    }
                }

                return false;

                bool IsAssignedValueOfRegisteredType(ISymbol memberSymbol, SyntaxNode declaration)
                {
                    if (declaration.TryFirstAncestor(out TypeDeclarationSyntax? typeDeclaration))
                    {
                        using var walker = AssignmentExecutionWalker.For(memberSymbol, typeDeclaration, SearchScope.Type, recursion.SemanticModel, recursion.CancellationToken);
                        foreach (var assignment in walker.Assignments)
                        {
                            if (!IsValueValidForRegisteredType(assignment.Right, registeredType, recursion))
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }

                bool IsReturnValueOfRegisteredType(SyntaxNode declaration)
                {
                    using var walker = ReturnValueWalker.Borrow(declaration);
                    foreach (var returnValue in walker.ReturnValues)
                    {
                        if (!IsValueValidForRegisteredType(returnValue, registeredType, recursion))
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
        }
    }
}
