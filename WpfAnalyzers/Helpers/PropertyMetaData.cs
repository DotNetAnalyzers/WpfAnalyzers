namespace WpfAnalyzers
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyMetadata
    {
        internal static bool TryGetConstructor(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, CancellationToken cancellationToken, out IMethodSymbol constructor)
        {
            return semanticModel.TryGetSymbol(objectCreation, KnownSymbols.PropertyMetadata, cancellationToken, out constructor) ||
                   semanticModel.TryGetSymbol(objectCreation, KnownSymbols.UIPropertyMetadata, cancellationToken, out constructor) ||
                   semanticModel.TryGetSymbol(objectCreation, KnownSymbols.FrameworkPropertyMetadata, cancellationToken, out constructor);
        }

        internal static bool TryGetDefaultValue(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax defaultValueArg)
        {
            defaultValueArg = null;
            if (objectCreation?.ArgumentList == null ||
                objectCreation.ArgumentList.Arguments.Count == 0)
            {
                return false;
            }

            return TryGetConstructor(objectCreation, semanticModel, cancellationToken, out var constructor) &&
                   constructor.Parameters.TryFirst(out var parameter) &&
                   parameter.Type == KnownSymbols.Object &&
                   objectCreation.ArgumentList.Arguments.TryFirst(out defaultValueArg);
        }

        internal static bool TryGetPropertyChangedCallback(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax callback)
        {
            return TryGetCallback(objectCreation, KnownSymbols.PropertyChangedCallback, semanticModel, cancellationToken, out callback);
        }

        internal static bool TryGetCoerceValueCallback(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax callback)
        {
            return TryGetCallback(objectCreation, KnownSymbols.CoerceValueCallback, semanticModel, cancellationToken, out callback);
        }

        internal static bool TryGetRegisteredName(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax nameArg, out string registeredName)
        {
            nameArg = null;
            registeredName = null;
            return TryGetConstructor(objectCreation, semanticModel, cancellationToken, out _) &&
                   objectCreation.TryFirstAncestor(out InvocationExpressionSyntax invocation) &&
                   DependencyProperty.TryGetRegisteredName(invocation, semanticModel, cancellationToken, out nameArg, out registeredName);
        }

        internal static bool TryGetDependencyProperty(ObjectCreationExpressionSyntax objectCreation, SemanticModel semanticModel, CancellationToken cancellationToken, out BackingFieldOrProperty fieldOrProperty)
        {
            fieldOrProperty = default;
            var invocation = objectCreation.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocation == null)
            {
                return false;
            }

            if (DependencyProperty.TryGetRegisterCall(invocation, semanticModel, cancellationToken, out _) ||
                DependencyProperty.TryGetRegisterReadOnlyCall(invocation, semanticModel, cancellationToken, out _) ||
                DependencyProperty.TryGetRegisterAttachedCall(invocation, semanticModel, cancellationToken, out _) ||
                DependencyProperty.TryGetRegisterAttachedReadOnlyCall(invocation, semanticModel, cancellationToken, out _))
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
                (DependencyProperty.TryGetAddOwnerCall(invocation, semanticModel, cancellationToken, out _) ||
                 DependencyProperty.TryGetOverrideMetadataCall(invocation, semanticModel, cancellationToken, out _)) &&
                semanticModel.TryGetSymbol(memberAccess.Expression, cancellationToken, out ISymbol candidate))
            {
                return BackingFieldOrProperty.TryCreateForDependencyProperty(candidate, out fieldOrProperty);
            }

            return false;
        }

        internal static bool IsValueValidForRegisteredType(ExpressionSyntax value, ITypeSymbol registeredType, SemanticModel semanticModel, CancellationToken cancellationToken, PooledSet<SyntaxNode> visited = null)
        {
            switch (value)
            {
                case ConditionalExpressionSyntax conditional:
                    return IsValueValidForRegisteredType(conditional.WhenTrue, registeredType, semanticModel, cancellationToken, visited) &&
                           IsValueValidForRegisteredType(conditional.WhenFalse, registeredType, semanticModel, cancellationToken, visited);

                case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.CoalesceExpression):
                    return IsValueValidForRegisteredType(binary.Left, registeredType, semanticModel, cancellationToken, visited) &&
                           IsValueValidForRegisteredType(binary.Right, registeredType, semanticModel, cancellationToken, visited);
            }

            if (registeredType.TypeKind == TypeKind.Enum)
            {
                return semanticModel.TryGetType(value, cancellationToken, out var valueType) &&
                       valueType.MetadataName == registeredType.MetadataName &&
                       Equals(valueType.ContainingType, registeredType.ContainingType) &&
                       NamespaceSymbolComparer.Equals(valueType.ContainingNamespace, registeredType.ContainingNamespace);
            }

            if (semanticModel.IsRepresentationPreservingConversion(value, registeredType))
            {
                return true;
            }

            if (semanticModel.TryGetSymbol(value, cancellationToken, out var symbol))
            {
                if (symbol is IFieldSymbol field)
                {
                    if (field.TrySingleDeclaration(cancellationToken, out var fieldDeclaration))
                    {
                        if (fieldDeclaration.Declaration is VariableDeclarationSyntax variableDeclaration &&
                            variableDeclaration.Variables.TryLast(out var variable) &&
                            variable.Initializer is EqualsValueClauseSyntax initializer &&
                            !IsValueValidForRegisteredType(initializer.Value, registeredType, semanticModel, cancellationToken, visited))
                        {
                            return false;
                        }

                        return IsAssignedValueOfRegisteredType(symbol, fieldDeclaration);
                    }

                    return field.Type == KnownSymbols.Object;
                }

                if (symbol is IPropertySymbol property)
                {
                    if (property.TrySingleDeclaration(cancellationToken, out PropertyDeclarationSyntax propertyDeclaration))
                    {
                        if (propertyDeclaration.Initializer is EqualsValueClauseSyntax initializer &&
                            !IsValueValidForRegisteredType(initializer.Value, registeredType, semanticModel, cancellationToken, visited))
                        {
                            return false;
                        }

                        if (property.SetMethod == null &&
                            property.GetMethod is IMethodSymbol getMethod)
                        {
                            return IsReturnValueOfRegisteredType(getMethod);
                        }

                        return IsAssignedValueOfRegisteredType(symbol, propertyDeclaration);
                    }

                    return property.Type == KnownSymbols.Object;
                }

                if (symbol is IMethodSymbol method)
                {
                    return IsReturnValueOfRegisteredType(method);
                }
            }

            return false;

            bool IsAssignedValueOfRegisteredType(ISymbol memberSymbol, MemberDeclarationSyntax declaration)
            {
                if (declaration.TryFirstAncestor(out TypeDeclarationSyntax typeDeclaration))
                {
                    using (var walker = AssignmentExecutionWalker.For(memberSymbol, typeDeclaration, Scope.Type, semanticModel, cancellationToken))
                    {
                        foreach (var assignment in walker.Assignments)
                        {
                            if (!IsValueValidForRegisteredType(assignment.Right, registeredType, semanticModel, cancellationToken, visited))
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }

            bool IsReturnValueOfRegisteredType(IMethodSymbol method)
            {
                if (method.TrySingleMethodDeclaration(cancellationToken, out var target))
                {
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
                    using (visited = visited.IncrementUsage())
#pragma warning restore IDISP003 // Dispose previous before re-assigning.
                    {
                        if (visited.Add(target))
                        {
                            using (var walker = ReturnValueWalker.Borrow(target))
                            {
                                foreach (var returnValue in walker.ReturnValues)
                                {
                                    if (!IsValueValidForRegisteredType(returnValue, registeredType, semanticModel, cancellationToken, visited))
                                    {
                                        return false;
                                    }
                                }
                            }
                        }

                        return true;
                    }
                }

                return method.ReturnType == KnownSymbols.Object;
            }
        }

        private static bool TryGetCallback(ObjectCreationExpressionSyntax objectCreation, QualifiedType callbackType, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax callback)
        {
            callback = null;
            return TryGetConstructor(objectCreation, semanticModel, cancellationToken, out var constructor) &&
                   constructor.TryFindParameter(callbackType, out var parameter) &&
                   objectCreation.TryFindArgument(parameter, out callback);
        }
    }
}
