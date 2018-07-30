namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class PropertyMetadataAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0005PropertyChangedCallbackShouldMatchRegisteredName.Descriptor,
            WPF0006CoerceValueCallbackShouldMatchRegisteredName.Descriptor,
            WPF0010DefaultValueMustMatchRegisteredType.Descriptor,
            WPF0016DefaultValueIsSharedReferenceType.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.ObjectCreationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is ObjectCreationExpressionSyntax objectCreation &&
                context.ContainingSymbol.IsStatic &&
                PropertyMetadata.TryGetConstructor(objectCreation, context.SemanticModel, context.CancellationToken, out _))
            {
                if (PropertyMetadata.TryGetRegisteredName(objectCreation, context.SemanticModel, context.CancellationToken, out var registeredName))
                {
                    if (PropertyMetadata.TryGetPropertyChangedCallback(objectCreation, context.SemanticModel, context.CancellationToken, out var propertyChangedCallback) &&
                        Callback.TryGetTarget(propertyChangedCallback, KnownSymbol.PropertyChangedCallback, context.SemanticModel, context.CancellationToken, out var identifier, out var target))
                    {
                        if (!target.Name.IsParts("On", registeredName, "Changed"))
                        {
                            context.ReportDiagnostic(
                            Diagnostic.Create(
                                WPF0005PropertyChangedCallbackShouldMatchRegisteredName.Descriptor,
                                identifier.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add("ExpectedName", $"On{registeredName}Changed"),
                                identifier,
                                $"On{registeredName}Changed"));
                        }
                    }

                    if (PropertyMetadata.TryGetCoerceValueCallback(objectCreation, context.SemanticModel, context.CancellationToken, out var coerceValueCallback) &&
                        Callback.TryGetTarget(coerceValueCallback, KnownSymbol.CoerceValueCallback, context.SemanticModel, context.CancellationToken, out identifier, out target) &&
                        !target.Name.IsParts("Coerce", registeredName))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                WPF0006CoerceValueCallbackShouldMatchRegisteredName.Descriptor,
                                identifier.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add("ExpectedName", $"Coerce{registeredName}"),
                                identifier,
                                $"Coerce{registeredName}"));
                    }
                }

                if (PropertyMetadata.TryGetDependencyProperty(objectCreation, context.SemanticModel, context.CancellationToken, out var fieldOrProperty) &&
                    DependencyProperty.TryGetRegisteredType(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredType) &&
                    PropertyMetadata.TryGetDefaultValue(objectCreation, context.SemanticModel, context.CancellationToken, out var defaultValueArg))
                {
                    if (!IsDefaultValueOfRegisteredType(defaultValueArg.Expression, registeredType, context))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                WPF0010DefaultValueMustMatchRegisteredType.Descriptor,
                                defaultValueArg.GetLocation(),
                                fieldOrProperty.Symbol,
                                registeredType));
                    }

                    if (registeredType.IsReferenceType)
                    {
                        var defaultValue = defaultValueArg.Expression;
                        if (defaultValue != null &&
                            !defaultValue.IsKind(SyntaxKind.NullLiteralExpression) &&
                            registeredType != KnownSymbol.FontFamily)
                        {
                            if (IsNonEmptyArrayCreation(defaultValue as ArrayCreationExpressionSyntax, context) ||
                                IsReferenceTypeCreation(defaultValue as ObjectCreationExpressionSyntax, context))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(WPF0016DefaultValueIsSharedReferenceType.Descriptor, defaultValueArg.GetLocation(), fieldOrProperty.Symbol));
                            }
                        }
                    }
                }
            }
        }

        private static bool IsDefaultValueOfRegisteredType(ExpressionSyntax defaultValue, ITypeSymbol registeredType, SyntaxNodeAnalysisContext context, PooledSet<SyntaxNode> visited = null)
        {
            switch (defaultValue)
            {
                case ConditionalExpressionSyntax conditional:
                    using (visited = visited.IncrementUsage())
                    {
                        return visited.Add(defaultValue) &&
                               IsDefaultValueOfRegisteredType(conditional.WhenTrue, registeredType, context, visited) &&
                               IsDefaultValueOfRegisteredType(conditional.WhenFalse, registeredType, context, visited);
                    }

                case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.CoalesceExpression):
                    using (visited = visited.IncrementUsage())
                    {
                        return visited.Add(defaultValue) &&
                               IsDefaultValueOfRegisteredType(binary.Left, registeredType, context, visited) &&
                               IsDefaultValueOfRegisteredType(binary.Right, registeredType, context, visited);
                    }
            }

            if (context.SemanticModel.IsRepresentationPreservingConversion(defaultValue, registeredType, context.CancellationToken))
            {
                return true;
            }

            if (context.SemanticModel.TryGetSymbol(defaultValue, context.CancellationToken, out ISymbol symbol))
            {
                if (symbol is IFieldSymbol field)
                {
                    if (field.TrySingleDeclaration(context.CancellationToken, out var fieldDeclaration))
                    {
                        using (visited = visited.IncrementUsage())
                        {
                            if (fieldDeclaration.Declaration is VariableDeclarationSyntax variableDeclaration &&
                                variableDeclaration.Variables.TryLast(out var variable) &&
                                variable.Initializer is EqualsValueClauseSyntax initializer)
                            {
                                return visited.Add(initializer.Value) &&
                                       IsDefaultValueOfRegisteredType(initializer.Value, registeredType, context, visited);
                            }

                            return fieldDeclaration.TryFirstAncestor<TypeDeclarationSyntax>(out var typeDeclaration) &&
                                   AssignmentExecutionWalker.SingleFor(symbol, typeDeclaration, Scope.Instance, context.SemanticModel, context.CancellationToken, out var assignedValue) &&
                                   visited.Add(assignedValue) &&
                                   IsDefaultValueOfRegisteredType(assignedValue, registeredType, context, visited);
                        }
                    }

                    return field.Type == KnownSymbol.Object;
                }

                if (symbol is IPropertySymbol property)
                {
                    if (property.TrySingleDeclaration(context.CancellationToken, out PropertyDeclarationSyntax propertyDeclaration))
                    {
                        using (visited = visited.IncrementUsage())
                        {
                            if (propertyDeclaration.Initializer is EqualsValueClauseSyntax initializer)
                            {
                                return visited.Add(initializer.Value) &&
                                       IsDefaultValueOfRegisteredType(initializer.Value, registeredType, context, visited);
                            }

                            if (property.SetMethod == null &&
                                property.GetMethod is IMethodSymbol getMethod)
                            {
                                return IsReturnValueOfRegisteredType(getMethod, visited);
                            }

                            return propertyDeclaration.TryFirstAncestor<TypeDeclarationSyntax>(out var typeDeclaration) &&
                                   AssignmentExecutionWalker.SingleFor(symbol, typeDeclaration, Scope.Instance, context.SemanticModel, context.CancellationToken, out var assignedValue) &&
                                   visited.Add(assignedValue) &&
                                   IsDefaultValueOfRegisteredType(assignedValue, registeredType, context, visited);
                        }
                    }

                    return property.Type == KnownSymbol.Object;
                }

                if (symbol is IMethodSymbol method)
                {
                    return IsReturnValueOfRegisteredType(method, visited);
                }
            }

            return false;

            bool IsReturnValueOfRegisteredType(IMethodSymbol method, PooledSet<SyntaxNode> v)
            {
                if (method.TrySingleMethodDeclaration(context.CancellationToken, out var target))
                {
                    using (var walker = ReturnValueWalker.Borrow(target))
                    {
                        foreach (var returnValue in walker.ReturnValues)
                        {
                            if (!IsDefaultValueOfRegisteredType(returnValue, registeredType, context, v))
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                }

                return method.ReturnType == KnownSymbol.Object;
            }
        }

        private static bool IsNonEmptyArrayCreation(ArrayCreationExpressionSyntax arrayCreation, SyntaxNodeAnalysisContext context)
        {
            if (arrayCreation == null)
            {
                return false;
            }

            foreach (var rank in arrayCreation.Type.RankSpecifiers)
            {
                foreach (var size in rank.Sizes)
                {
                    var constantValue = context.SemanticModel.GetConstantValueSafe(size, context.CancellationToken);
                    if (!constantValue.HasValue)
                    {
                        return true;
                    }

                    return !Equals(constantValue.Value, 0);
                }
            }

            return false;
        }

        private static bool IsReferenceTypeCreation(ObjectCreationExpressionSyntax objectCreation, SyntaxNodeAnalysisContext context)
        {
            if (objectCreation == null)
            {
                return false;
            }

            var type = context.SemanticModel.GetTypeInfoSafe(objectCreation, context.CancellationToken)
                              .Type;
            if (type.IsValueType)
            {
                return false;
            }

            return true;
        }
    }
}
