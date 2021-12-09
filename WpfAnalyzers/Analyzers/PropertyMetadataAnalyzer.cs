namespace WpfAnalyzers
{
    using System;
    using System.Collections.Immutable;

    using Gu.Roslyn.AnalyzerExtensions;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class PropertyMetadataAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.WPF0005PropertyChangedCallbackShouldMatchRegisteredName,
            Descriptors.WPF0006CoerceValueCallbackShouldMatchRegisteredName,
            Descriptors.WPF0010DefaultValueMustMatchRegisteredType,
            Descriptors.WPF0016DefaultValueIsSharedReferenceType,
            Descriptors.WPF0023ConvertToLambda,
            Descriptors.WPF0024ParameterShouldBeNullable);

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
                context.ContainingSymbol is { IsStatic: true } &&
                PropertyMetadata.Match(objectCreation, context.SemanticModel, context.CancellationToken) is { } propertyMetadata)
            {
                if (propertyMetadata.FindRegisteredName(context.SemanticModel, context.CancellationToken) is { Value: { } registeredName })
                {
                    if (propertyMetadata.PropertyChangedArgument is { } propertyChangeArgument &&
                        Callback.Match(propertyChangeArgument, KnownSymbols.PropertyChangedCallback, context.SemanticModel, context.CancellationToken) is { Identifier: { } onPropertyChangedNode, Target: { } onPropertyChanged })
                    {
                        if (TypeSymbolComparer.Equal(onPropertyChanged.ContainingType, context.ContainingSymbol.ContainingType) &&
                            !onPropertyChanged.Name.IsParts("On", registeredName, "Changed") &&
                            onPropertyChanged.DeclaredAccessibility.IsEither(Accessibility.Private, Accessibility.Protected))
                        {
                            using var walker = InvocationWalker.InContainingClass(onPropertyChanged, context.SemanticModel, context.CancellationToken);
                            if (walker.IdentifierNames.Count == 1)
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        Descriptors.WPF0005PropertyChangedCallbackShouldMatchRegisteredName,
                                        onPropertyChangedNode.GetLocation(),
                                        ImmutableDictionary<string, string?>.Empty.Add("ExpectedName", $"On{registeredName}Changed"),
                                        onPropertyChangedNode,
                                        $"On{registeredName}Changed"));
                            }
                            else if (onPropertyChanged.Name.StartsWith("On", StringComparison.Ordinal) &&
                                     onPropertyChanged.Name.EndsWith("Changed", StringComparison.Ordinal))
                            {
                                foreach (var identifierName in walker.IdentifierNames)
                                {
                                    if (identifierName.TryFirstAncestor(out ArgumentSyntax? argument) &&
                                        argument != propertyChangeArgument &&
                                        MatchesPropertyChangedCallbackName(argument, onPropertyChanged, context))
                                    {
                                        context.ReportDiagnostic(
                                            Diagnostic.Create(
                                                Descriptors.WPF0005PropertyChangedCallbackShouldMatchRegisteredName,
                                                onPropertyChangedNode.GetLocation(),
                                                onPropertyChangedNode,
                                                $"On{registeredName}Changed"));
                                        break;
                                    }
                                }
                            }
                        }

                        if (onPropertyChanged.TrySingleMethodDeclaration(context.CancellationToken, out var declaration) &&
                            Callback.CanInlineBody(declaration))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0023ConvertToLambda, propertyChangeArgument.GetLocation()));
                        }
                    }

                    if (propertyMetadata.CoerceValueArgument is { } coerceValueArgument &&
                        Callback.Match(coerceValueArgument, KnownSymbols.CoerceValueCallback, context.SemanticModel, context.CancellationToken) is { Identifier: { } coerceNode, Target: { } coerce })
                    {
                        if (TypeSymbolComparer.Equal(coerce.ContainingType, context.ContainingSymbol.ContainingType) &&
                            !coerce.Name.IsParts("Coerce", registeredName))
                        {
                            using var walker = InvocationWalker.InContainingClass(coerce, context.SemanticModel, context.CancellationToken);
                            if (walker.IdentifierNames.Count == 1)
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        Descriptors.WPF0006CoerceValueCallbackShouldMatchRegisteredName,
                                        coerceNode.GetLocation(),
                                        ImmutableDictionary<string, string?>.Empty.Add("ExpectedName", $"Coerce{registeredName}"),
                                        coerceNode,
                                        $"Coerce{registeredName}"));
                            }
                            else if (coerce.Name.StartsWith("Coerce", StringComparison.Ordinal))
                            {
                                foreach (var identifierName in walker.IdentifierNames)
                                {
                                    if (identifierName.TryFirstAncestor(out ArgumentSyntax? argument) &&
                                        argument != coerceValueArgument &&
                                        MatchesCoerceValueCallbackName(argument, coerce, context))
                                    {
                                        context.ReportDiagnostic(
                                            Diagnostic.Create(
                                                Descriptors.WPF0006CoerceValueCallbackShouldMatchRegisteredName,
                                                coerceNode.GetLocation(),
                                                coerceNode,
                                                $"Coerce{registeredName}"));
                                        break;
                                    }
                                }
                            }
                        }

                        if (coerce.TrySingleMethodDeclaration(context.CancellationToken, out var declaration))
                        {
                            if (Callback.CanInlineBody(declaration))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0023ConvertToLambda, coerceValueArgument.GetLocation()));
                            }

                            if (context.SemanticModel.GetNullableContext(declaration.SpanStart).HasFlag(NullableContext.Enabled) &&
                                declaration.ParameterList is { Parameters: { Count: 2 } parameters } &&
                                parameters[1] is { Type: { } type and not NullableTypeSyntax })
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0024ParameterShouldBeNullable, type.GetLocation()));
                            }
                        }
                    }
                }

                if (PropertyMetadata.TryGetDependencyProperty(objectCreation, context.SemanticModel, context.CancellationToken) is { } backing &&
                    backing.RegisteredType(context.SemanticModel, context.CancellationToken) is { Value: { } registeredType } &&
                    propertyMetadata.DefaultValueArgument is { } defaultValueArg)
                {
                    if (!PropertyMetadata.IsValueValidForRegisteredType(defaultValueArg.Expression, registeredType, context.SemanticModel, context.CancellationToken))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0010DefaultValueMustMatchRegisteredType,
                                defaultValueArg.GetLocation(),
                                backing.Symbol,
                                registeredType));
                    }

                    if (registeredType.IsReferenceType &&
                        registeredType != KnownSymbols.FontFamily)
                    {
                        if ((defaultValueArg is { Expression: ArrayCreationExpressionSyntax defaultArray } &&
                             IsNonEmptyArrayCreation(defaultArray, context)) ||
                            (defaultValueArg is { Expression: ObjectCreationExpressionSyntax defaultInstance } &&
                             IsReferenceTypeCreation(defaultInstance, context)))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0016DefaultValueIsSharedReferenceType, defaultValueArg.GetLocation(), backing.Symbol));
                        }
                    }
                }
            }
        }

        private static bool MatchesPropertyChangedCallbackName(ArgumentSyntax propertyChangedCallback, IMethodSymbol target, SyntaxNodeAnalysisContext context)
        {
            return propertyChangedCallback is { Parent: ArgumentListSyntax { Parent: ObjectCreationExpressionSyntax objectCreation } } &&
                   PropertyMetadata.Match(objectCreation, context.SemanticModel, context.CancellationToken) is { } propertyMetadata &&
                   propertyMetadata.FindRegisteredName(context.SemanticModel, context.CancellationToken) is { Value: { } registeredName } &&
                   TypeSymbolComparer.Equal(target.ContainingType, context.ContainingSymbol?.ContainingType) &&
                   target.Name.IsParts("On", registeredName, "Changed");
        }

        private static bool MatchesCoerceValueCallbackName(ArgumentSyntax coerceValueCallback, IMethodSymbol target, SyntaxNodeAnalysisContext context)
        {
            return coerceValueCallback is { Parent: ArgumentListSyntax { Parent: ObjectCreationExpressionSyntax objectCreation } } &&
                   PropertyMetadata.Match(objectCreation, context.SemanticModel, context.CancellationToken) is { } propertyMetadata &&
                   propertyMetadata.FindRegisteredName(context.SemanticModel, context.CancellationToken) is { Value: { } registeredName } &&
                   TypeSymbolComparer.Equal(target.ContainingType, context.ContainingSymbol?.ContainingType) &&
                   target.Name.IsParts("Coerce", registeredName);
        }

        private static bool IsNonEmptyArrayCreation(ArrayCreationExpressionSyntax arrayCreation, SyntaxNodeAnalysisContext context)
        {
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
            return context.SemanticModel.TryGetType(objectCreation, context.CancellationToken, out var type) &&
                   !type.IsValueType;
        }
    }
}
