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
            Descriptors.WPF0005PropertyChangedCallbackShouldMatchRegisteredName,
            Descriptors.WPF0006CoerceValueCallbackShouldMatchRegisteredName,
            Descriptors.WPF0010DefaultValueMustMatchRegisteredType,
            WPF0016DefaultValueIsSharedReferenceType.Descriptor,
            WPF0023ConvertToLambda.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(c => Handle(c), SyntaxKind.ObjectCreationExpression);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is ObjectCreationExpressionSyntax objectCreation &&
                context.ContainingSymbol.IsStatic &&
                PropertyMetadata.TryGetConstructor(objectCreation, context.SemanticModel, context.CancellationToken, out _))
            {
                if (PropertyMetadata.TryGetRegisteredName(objectCreation, context.SemanticModel, context.CancellationToken, out var registeredName))
                {
                    if (PropertyMetadata.TryGetPropertyChangedCallback(objectCreation, context.SemanticModel, context.CancellationToken, out var propertyChangedCallback) &&
                        Callback.TryGetTarget(propertyChangedCallback, KnownSymbol.PropertyChangedCallback, context.SemanticModel, context.CancellationToken, out var callbackIdentifier, out var target))
                    {
                        if (target.ContainingType.Equals(context.ContainingSymbol.ContainingType) &&
                            !target.Name.IsParts("On", registeredName, "Changed") &&
                            target.DeclaredAccessibility.IsEither(Accessibility.Private, Accessibility.Protected))
                        {
                            using (var walker = InvocationWalker.InContainingClass(target, context.SemanticModel, context.CancellationToken))
                            {
                                if (walker.IdentifierNames.Count == 1)
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            Descriptors.WPF0005PropertyChangedCallbackShouldMatchRegisteredName,
                                            callbackIdentifier.GetLocation(),
                                            ImmutableDictionary<string, string>.Empty.Add("ExpectedName", $"On{registeredName}Changed"),
                                            callbackIdentifier,
                                            $"On{registeredName}Changed"));
                                }
                                else if (target.Name.StartsWith("On") &&
                                         target.Name.EndsWith("Changed"))
                                {
                                    foreach (var identifierName in walker.IdentifierNames)
                                    {
                                        if (identifierName.TryFirstAncestor(out ArgumentSyntax argument) &&
                                            argument != propertyChangedCallback &&
                                            MatchesPropertyChangedCallbackName(argument, target, context))
                                        {
                                            context.ReportDiagnostic(
                                                Diagnostic.Create(
                                                    Descriptors.WPF0005PropertyChangedCallbackShouldMatchRegisteredName,
                                                    callbackIdentifier.GetLocation(),
                                                    callbackIdentifier,
                                                    $"On{registeredName}Changed"));
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (target.TrySingleMethodDeclaration(context.CancellationToken, out var declaration) &&
                            Callback.IsSingleExpression(declaration))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(WPF0023ConvertToLambda.Descriptor, propertyChangedCallback.GetLocation()));
                        }
                    }

                    if (PropertyMetadata.TryGetCoerceValueCallback(objectCreation, context.SemanticModel, context.CancellationToken, out var coerceValueCallback) &&
                        Callback.TryGetTarget(coerceValueCallback, KnownSymbol.CoerceValueCallback, context.SemanticModel, context.CancellationToken, out callbackIdentifier, out target))
                    {
                        if (target.ContainingType.Equals(context.ContainingSymbol.ContainingType) &&
                            !target.Name.IsParts("Coerce", registeredName))
                        {
                            using (var walker = InvocationWalker.InContainingClass(target, context.SemanticModel, context.CancellationToken))
                            {
                                if (walker.IdentifierNames.Count == 1)
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            Descriptors.WPF0006CoerceValueCallbackShouldMatchRegisteredName,
                                            callbackIdentifier.GetLocation(),
                                            ImmutableDictionary<string, string>.Empty.Add("ExpectedName", $"Coerce{registeredName}"),
                                            callbackIdentifier,
                                            $"Coerce{registeredName}"));
                                }
                                else if (target.Name.StartsWith("Coerce"))
                                {
                                    foreach (var identifierName in walker.IdentifierNames)
                                    {
                                        if (identifierName.TryFirstAncestor(out ArgumentSyntax argument) &&
                                            argument != propertyChangedCallback &&
                                            MatchesCoerceValueCallbackName(argument, target, context))
                                        {
                                            context.ReportDiagnostic(
                                                Diagnostic.Create(
                                                    Descriptors.WPF0006CoerceValueCallbackShouldMatchRegisteredName,
                                                    callbackIdentifier.GetLocation(),
                                                    callbackIdentifier,
                                                    $"Coerce{registeredName}"));
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (target.TrySingleMethodDeclaration(context.CancellationToken, out var declaration) &&
                            Callback.IsSingleExpression(declaration))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(WPF0023ConvertToLambda.Descriptor, propertyChangedCallback.GetLocation()));
                        }
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
                                Descriptors.WPF0010DefaultValueMustMatchRegisteredType,
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

        private static bool MatchesPropertyChangedCallbackName(ArgumentSyntax propertyChangedCallback, IMethodSymbol target, SyntaxNodeAnalysisContext context)
        {
            return propertyChangedCallback.Parent is ArgumentListSyntax argumentList &&
                   argumentList.Parent is ObjectCreationExpressionSyntax objectCreation &&
                   PropertyMetadata.TryGetRegisteredName(objectCreation, context.SemanticModel, context.CancellationToken, out var registeredName) &&
                   target.ContainingType.Equals(context.ContainingSymbol.ContainingType) &&
                   target.Name.IsParts("On", registeredName, "Changed");
        }

        private static bool MatchesCoerceValueCallbackName(ArgumentSyntax coerceValueCallback, IMethodSymbol target, SyntaxNodeAnalysisContext context)
        {
            return coerceValueCallback.Parent is ArgumentListSyntax argumentList &&
                   argumentList.Parent is ObjectCreationExpressionSyntax objectCreation &&
                   PropertyMetadata.TryGetRegisteredName(objectCreation, context.SemanticModel, context.CancellationToken, out var registeredName) &&
                   target.ContainingType.Equals(context.ContainingSymbol.ContainingType) &&
                   target.Name.IsParts("Coerce", registeredName);
        }

        private static bool IsDefaultValueOfRegisteredType(ExpressionSyntax defaultValue, ITypeSymbol registeredType, SyntaxNodeAnalysisContext context, PooledSet<SyntaxNode> visited = null)
        {
            switch (defaultValue)
            {
                case ConditionalExpressionSyntax conditional:
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
                    using (visited = visited.IncrementUsage())
#pragma warning restore IDISP003 // Dispose previous before re-assigning.
                    {
                        return visited.Add(defaultValue) &&
                               IsDefaultValueOfRegisteredType(conditional.WhenTrue, registeredType, context, visited) &&
                               IsDefaultValueOfRegisteredType(conditional.WhenFalse, registeredType, context, visited);
                    }

                case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.CoalesceExpression):
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
                    using (visited = visited.IncrementUsage())
#pragma warning restore IDISP003 // Dispose previous before re-assigning.
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
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
                        using (visited = visited.IncrementUsage())
#pragma warning restore IDISP003 // Dispose previous before re-assigning.
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
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
                        using (visited = visited.IncrementUsage())
#pragma warning restore IDISP003 // Dispose previous before re-assigning.
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
